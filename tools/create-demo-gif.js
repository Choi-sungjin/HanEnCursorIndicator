// Compatibility entry point for the current, privacy-safe README demo.
const { spawnSync } = require("node:child_process");
const path = require("node:path");
const result = spawnSync(process.env.HANEN_DEMO_PYTHON || "python", [path.join(__dirname, "create-readme-demo.py")], { stdio: "inherit" });
if (result.error) { console.error(result.error.message); process.exit(1); }
process.exit(result.status === null ? 1 : result.status);
