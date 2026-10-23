# Spiral Shuffle website — GitHub Pages

This directory is a **static website** for Spiral Shuffle. It runs without npm, Node, server-side software or an asset build.

## Publish

The site is committed to the existing public repository, `HyperColossus/spiral-shuffle`.

1. Open https://github.com/HyperColossus/spiral-shuffle/settings/pages
2. Under **Build and deployment**, choose **Deploy from a branch**.
3. Set branch **main**, folder **/docs**, then **Save**.
4. After the first deployment, the expected address is:
   https://spiral.hypercoloss.us/

GitHub Pages is configured with a project-specific CNAME file, but the domain requires the DNS record below and confirmation in Pages settings.

## Spiral Shuffle custom subdomain

- Site hostname: **spiral.hypercoloss.us** (no `/spiral-shuffle/` path).
- Publishing source: **main /docs**, CNAME file: `docs/CNAME`.
- At the DNS provider for `hypercoloss.us`, create a **CNAME** named `spiral` pointing to **`hypercolossus.github.io`** (no path, no protocol). Do not alter existing `@` or `www` DNS records for the portfolio.
- In https://github.com/HyperColossus/spiral-shuffle/settings/pages confirm **Custom domain** is `spiral.hypercoloss.us`, run **Check DNS** if offered, then use **Enforce HTTPS** after the certificate is available. GitHub's settings may be needed even when a CNAME file has been committed.
- DNS changes can take up to 24 hours to resolve.

## Edit the public-facing connection details

Open `docs/index.html` in GitHub and locate `window.SPIRAL_SITE_CONFIG` near the bottom:

```js
window.SPIRAL_SITE_CONFIG = {
  gameHost: "",
  gamePort: 12010,
  registrationApi: "",
  turnstileSiteKey: ""
};
```

- **gameHost:** public IPv4 or hostname of the **Ubuntu game server** (not your Windows PC and not a Tailscale address). Leave empty until the internet-facing game server is actually reachable.
- **gamePort:** currently `12010` for game login. Realm ports `12343–12348` must separately be accessible.
- **registrationApi:** leave empty for a UI-only site. Set it to the **public HTTPS API URL** after your server-side registration API is deployed and tested (e.g. `https://register.example.com/register`).
- **turnstileSiteKey:** leave empty until Cloudflare Turnstile has been configured for your GitHub Pages hostname. This is a public site key, **not the secret**. Both values must be populated for the form to become active.

GitHub Pages cannot run Python, PHP, ASP.NET, a game server or a password database. It can render the page and send browser requests to an independently hosted HTTPS registration service.

## Registration API contract (future integration)

Frontend request:

```http
POST /register
Content-Type: application/json

{"username":"player_123","password":"unique-long-password","turnstileToken":"CLOUDFLARE_TOKEN"}
```

- Successful account creation: return status `201` (or another 2xx), optionally JSON `{"message":"Created"}`.
- Validation error: return 4xx and JSON `{"error":"Safe public error text"}`.
- Backend must use Imlight's account-creation API / native password hashing and create **AuthLevel.None** accounts, not independent web-only accounts.
- HTTPS and narrow `Access-Control-Allow-Origin` to the exact GitHub Pages origin (or eventual custom domain) are required. Handle browser preflight `OPTIONS`. Do **not** use `Access-Control-Allow-Origin: *` with a privileged registration API.
- Add rate limits per IP and per account name, bot protection, abuse prevention, and logs without plaintext passwords. Enforce inputs server-side regardless of client-side checks.
- Do not put tokens, secrets, administrator credentials, private API keys, database URLs, or server filesystem paths into this HTML/repository.
- Do not enable the public registration API before securing the game server's temporary admin password, deciding which RavenDB save directory is permanent, and reviewing public-exposure risks.

When `registrationApi` is empty, password inputs and submission are disabled; the site **does not claim to create accounts**.

## Local preview

Open `docs/index.html` in a browser. No backend is needed to preview the design. Use an HTTPS-hosted site for live registrations.

## Branding

Independent fan-made project. Not affiliated with, approved by, or endorsed by KingsIsle Entertainment. The site does not include or distribute a Wizard101 executable or copyrighted game files.

## Backend

The backend source, Imlight registration bridge, Ubuntu installation instructions, automated tests and HTTPS proxy configuration are included in this same repository. See [README-PORTAL.md](README-PORTAL.md). The API is not deployed merely by publishing GitHub Pages.
