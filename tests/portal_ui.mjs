// Website behavior smoke checks with a DOM/Turnstile/fetch harness (no network).
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import vm from 'node:vm';

const html = readFileSync(new URL('../docs/index.html', import.meta.url), 'utf8');
const scripts = [...html.matchAll(/<script>([\s\S]*?)<\/script>/g)].map(match => match[1]);
assert.equal(scripts.length, 2);
function page(config) {
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
    getResponse() { return 'captcha-proof'; }, reset() {},
  }};
  const document = {
    getElementById: get, createElement: () => ({}),
    head: { appendChild(script) { script.onload(); } },
  };
  const context = vm.createContext({ window, document, URL, fetch: async (url, options) => {
    requests.push({ url, body: JSON.parse(options.body) });
    return { ok: true, json: async () => ({ message: 'Queued.', accessKey: 'a'.repeat(43) }) };
  }});
  vm.runInContext(scripts[0], context);
  Object.assign(window.SPIRAL_SITE_CONFIG, config);
  vm.runInContext(scripts[1], context);
  return { get, widgets, requests };
}

const locked = page({});
assert.equal(locked.widgets.length, 0);
assert.equal(locked.get('create-account').disabled, true);
assert.equal(locked.get('access-controls').hidden, true);

const enabled = page({ registrationApi: 'https://register.example/register',
  turnstileSiteKey: 'example-public-site-key', ipAccessEnabled: true });
assert.equal(enabled.widgets.length, 1);
assert.equal(enabled.get('create-account').disabled, false);
enabled.get('username').value = 'new_player';
enabled.get('password').value = 'Example-Private-Password';
await enabled.get('signup-form').events.submit({ preventDefault() {} });
assert.equal(enabled.get('new-access-key').value, 'a'.repeat(43));
assert.equal(enabled.get('saved-access').hidden, false);
assert.equal(enabled.get('status').textContent, 'Queued.');
enabled.get('access-controls').open = true;
enabled.get('access-controls').events.toggle();
assert.equal(enabled.widgets.length, 2);
enabled.get('access-controls').events.toggle();
assert.equal(enabled.widgets.length, 2);
enabled.get('access-key').value = 'a'.repeat(43);
await enabled.get('access-form').events.submit({ preventDefault() {} });
assert.equal(enabled.requests[1].url, 'https://register.example/access/refresh');
assert.deepEqual(enabled.requests[1].body, { accessKey: 'a'.repeat(43), turnstileToken: 'captcha-proof' });
assert.equal(enabled.get('access-status').textContent, 'Queued.');
assert.equal(enabled.get('refresh-access').disabled, false);
console.log('PASS: disabled defaults, signup key display, lazy verification, and access refresh.');
