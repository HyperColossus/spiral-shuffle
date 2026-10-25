// Website behavior smoke checks with a DOM/Turnstile/fetch harness (no network).
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import vm from 'node:vm';

const html = readFileSync(new URL('../docs/index.html', import.meta.url), 'utf8');
const scripts = [...html.matchAll(/<script>([\s\S]*?)<\/script>/g)].map(match => match[1]);
assert.equal(scripts.length, 2);
function page(config, mode = 'success') {
  let executes = 0;
  const elements = new Map();
  const get = id => {
    if (!elements.has(id)) elements.set(id, {
      value: '', disabled: true, hidden: true, open: false, textContent: '', events: {},
      classList: { add() {}, remove() {}, toggle() {} },
      addEventListener(name, callback) { this.events[name] = callback; },
      getContext() { return null; }, reset() {},
    });
    return elements.get(id);
  };
  const widgets = [];
  const requests = [];
  const window = { turnstile: {
    render(target, options) { widgets.push({ target, options }); return widgets.length - 1; },
    execute(id) {
      executes++;
      if (mode === 'defer') return;
      if (mode === 'error') return widgets[id].options['error-callback']('test-error');
      return widgets[id].options.callback('captcha-proof');
    }, reset() {},
  }};
  const document = {
    getElementById: get, createElement: () => ({}),
    head: { appendChild(script) { script.onload(); } },
  };
  const context = vm.createContext({ window, document, URL, fetch: async (url, options) => {
    requests.push({ url, body: JSON.parse(options.body) });
    return { ok: true, json: async () => ({ message: 'Queued.' }) };
  }});
  vm.runInContext(scripts[0], context);
  Object.assign(window.SPIRAL_SITE_CONFIG, config);
  vm.runInContext(scripts[1], context);
  return { get, widgets, requests, get executions() { return executes; } };
}

const locked = page({});
assert.equal(locked.widgets.length, 0);
assert.equal(locked.get('create-account').disabled, true);
assert.ok(!html.includes('id="access-key"'));
assert.ok(!html.includes('id="saved-access"'));

const enabled = page({ registrationApi: 'https://register.example/register',
  turnstileSiteKey: 'example-public-site-key' });
assert.equal(enabled.widgets.length, 1);
assert.equal(enabled.get('create-account').disabled, false);
enabled.get('username').value = 'new_player';
enabled.get('password').value = 'Example-Private-Password';
await enabled.get('signup-form').events.submit({ preventDefault() {} });
assert.equal(enabled.get('status').textContent, 'Credentials accepted.');
assert.deepEqual(enabled.requests[0].body, { username: 'new_player', password: 'Example-Private-Password', turnstileToken: 'captcha-proof' });
// The same two fields also refresh an existing player's access.
enabled.get('password').value = 'legacy';
await enabled.get('signup-form').events.submit({ preventDefault() {} });
assert.equal(enabled.requests[1].body.password, 'legacy');
assert.equal(enabled.widgets.length, 1);
assert.equal(enabled.get('create-account').disabled, false);
console.log('PASS: disabled defaults and one username/password form for new and existing players; no access-key UI.');

const config = { registrationApi: 'https://register.example/register', turnstileSiteKey: 'example-public-site-key' };
const deferred = page(config, 'defer');
deferred.get('username').value = 'new_player';
deferred.get('password').value = 'Example-Private-Password';
await deferred.get('signup-form').events.submit({ preventDefault() {} });
assert.equal(deferred.widgets[0].options.execution, 'execute');
assert.equal(deferred.executions, 1);
assert.equal(deferred.requests.length, 0);
assert.equal(deferred.get('create-account').disabled, true);
await deferred.get('signup-form').events.submit({ preventDefault() {} });
assert.equal(deferred.executions, 1);
await deferred.widgets[0].options.callback('verified-proof');
assert.equal(deferred.requests.length, 1);
assert.equal(deferred.requests[0].body.turnstileToken, 'verified-proof');
assert.equal(deferred.get('create-account').disabled, false);
await deferred.get('signup-form').events.submit({ preventDefault() {} });
deferred.widgets[0].options['expired-callback']();
assert.equal(deferred.requests.length, 1);
assert.equal(deferred.get('create-account').disabled, false);
const failed = page(config, 'error');
failed.get('username').value = 'new_player';
failed.get('password').value = 'Example-Private-Password';
await failed.get('signup-form').events.submit({ preventDefault() {} });
assert.equal(failed.requests.length, 0);
assert.equal(failed.get('create-account').disabled, false);
assert.match(failed.get('status').textContent, /Verification failed/);
console.log('PASS: Continue executes verification, waits for proof, prevents duplicate submits, and recovers after expiry/error.');

assert.ok(html.includes('autocomplete="on" novalidate'));
for (const [username, password, message] of [
  ['', 'ExamplePassword123!', 'Enter a username.'],
  ['ab', 'ExamplePassword123!', 'between 3 and 20'],
  ['1player', 'ExamplePassword123!', 'start with a letter'],
  ['my-player', 'ExamplePassword123!', 'only letters, numbers and underscores'],
  ['player_one', '', 'Enter your password.'],
  ['player_one', 'has spaces', 'cannot contain spaces'],
]) {
  const invalid = page(config);
  invalid.get('username').value = username;
  invalid.get('password').value = password;
  await invalid.get('signup-form').events.submit({ preventDefault() {} });
  assert.ok(invalid.get('status').textContent.includes(message));
  assert.equal(invalid.requests.length, 0);
  assert.equal(invalid.executions, 0);
}
const normalized = page(config);
normalized.get('username').value = 'Test_Player';
normalized.get('password').value = 'ExamplePassword123!';
await normalized.get('signup-form').events.submit({ preventDefault() {} });
assert.equal(normalized.requests[0].body.username, 'test_player');
console.log('PASS: specific field rejection messages and normalized usernames before verification.');
