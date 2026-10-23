# Spiral Shuffle website — GitHub Pages

This directory is a **static website** for Spiral Shuffle. It runs without npm, Node, server-side software or an asset build.

## Publish

The site is committed to the existing public repository, \`HyperColossus/spiral-shuffle\`.

1. Open https://github.com/HyperColossus/spiral-shuffle/settings/pages
2. Under **Build and deployment**, choose **Deploy from a branch**.
3. Set branch **main**, folder **/docs**, then **Save**.
4. After the first deployment, the expected address is:
   https://hypercolossus.github.io/spiral-shuffle/

This GitHub Pages setting requires action by the repository owner. The site is **not live** solely because these files are committed.

## Edit the public-facing connection details

Open \`docs/index.html\` in GitHub and locate \`window.SPIRAL_SITE_CONFIG\` near the bottom:

\`\`\`js
window.SPIRAL_SITE_CONFIG = {
  gameHost: "",
  gamePort: 12010,
  registrationApi: ""
};
\`\`\`

- **gameHost:** public IPv4 or hostname of the **Ubuntu game server** (not your Windows PC and not a Tailscale address). Leave empty until the internet-facing game server is actually reachable.
- **gamePort:** currently \`12010\` for game login. Realm ports \`12343–12348\` must separately be accessible.
- **registrationApi:** leave empty for a UI-only site. Set it to a **public HTTPS API URL** (e.g. \`https://api.example.com/register\`) after a secure, independently hosted registration backend has been deployed and tested. Never point it at private RavenDB or its 8090 port.

GitHub Pages cannot run Python, PHP, ASP.NET, a game server or a password database. It can render the page and send browser requests to an independently hosted HTTPS registration service.

## Registration API contract (future integration)

Frontend request:

\`\`\`http
POST /register
Content-Type: application/json

{"username":"Player_123","password":"unique-long-password"}
\`\`\`

- Successful account creation: return status \`201\` (or another 2xx), optionally JSON \`{"message":"Created"}\`.
- Validation error: return 4xx and JSON \`{"error":"Safe public error text"}\`.
- Backend must use Imlight's account-creation API / native password hashing and create **AuthLevel.None** accounts, not independent web-only accounts.
- HTTPS and narrow \`Access-Control-Allow-Origin\` to the exact GitHub Pages origin (or eventual custom domain) are required. Handle browser preflight \`OPTIONS\`. Do **not** use \`Access-Control-Allow-Origin: *\` with a privileged registration API.
- Add rate limits per IP and per account name, bot protection, abuse prevention, and logs without plaintext passwords. Enforce inputs server-side regardless of client-side checks.
- Do not put tokens, secrets, administrator credentials, private API keys, database URLs, or server filesystem paths into this HTML/repository.
- Do not enable the public registration API before securing the game server's temporary admin password, deciding which RavenDB save directory is permanent, and reviewing public-exposure risks.

When \`registrationApi\` is empty, password inputs and submission are disabled; the site **does not claim to create accounts**.

## Local preview

Open \`docs/index.html\` in a browser. No backend is needed to preview the design. Use an HTTPS-hosted site for live registrations.

## Branding

Independent fan-made project. Not affiliated with, approved by, or endorsed by KingsIsle Entertainment. The site does not include or distribute a Wizard101 executable or copyrighted game files.
