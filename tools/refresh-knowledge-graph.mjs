import { execFileSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";

const root = path.resolve(process.argv[2] || ".");
const uaDir = path.join(root, ".understand-anything");
const graphPath = path.join(uaDir, "knowledge-graph.json");
const oldGraph = fs.existsSync(graphPath)
  ? JSON.parse(fs.readFileSync(graphPath, "utf8"))
  : { nodes: [], edges: [] };

const tracked = execFileSync("git", ["ls-files", "--cached", "--others", "--exclude-standard"], {
  cwd: root,
  encoding: "utf8",
}).split(/\r?\n/).filter(Boolean).map((p) => p.replaceAll("\\", "/"));

const include = (p) =>
  p === "README.md" ||
  p === "AGENTS.md" ||
  p === ".gitattributes" ||
  p === ".markdownlint.jsonc" ||
  p === "Packages/manifest.json" ||
  p === "Packages/packages-lock.json" ||
  /^docs\/.*\.md$/i.test(p) ||
  /^tools\/.*\.(ps1|mjs)$/i.test(p) ||
  /^Assets\/.*\.(cs|unity|prefab)$/i.test(p) ||
  /^Assets\/AddressableAssetsData\/.*\.asset$/i.test(p) ||
  /^ProjectSettings\/(ProjectVersion\.txt|EditorBuildSettings\.asset|InputManager\.asset|ProjectSettings\.asset|GraphicsSettings\.asset|QualitySettings\.asset)$/i.test(p);

const files = tracked.filter(include).sort((a, b) => a.localeCompare(b));
const fileSet = new Set(files);
const oldByPath = new Map(oldGraph.nodes.filter((n) => n.filePath).map((n) => [n.filePath.replaceAll("\\", "/"), n]));
const prefixFor = (p) => {
  if (/\.md$/i.test(p)) return "document";
  if (/^(ProjectSettings|Packages)\//.test(p) || /\.(json|jsonc|asset|txt)$/i.test(p)) return "config";
  return "file";
};
const idFor = (p) => `${prefixFor(p)}:${p}`;
const languageFor = (p) => {
  const ext = path.extname(p).toLowerCase();
  return ({ ".cs": "csharp", ".md": "markdown", ".json": "json", ".jsonc": "json", ".ps1": "powershell", ".mjs": "javascript", ".unity": "unity", ".prefab": "unity", ".asset": "unity", ".txt": "text" })[ext] || "text";
};
const summaryFor = (p) => {
  const absolute = path.join(root, p);
  if (/\.md$/i.test(p)) {
    const lines = fs.readFileSync(absolute, "utf8").split(/\r?\n/);
    const title = lines.find((line) => /^# /.test(line))?.replace(/^# /, "").trim();
    const paragraph = lines.find((line) => line.trim() && !/^(#|>|\[|\*\*|---|<a )/.test(line.trim()));
    return `Tài liệu ${title || path.basename(p)}${paragraph ? `: ${paragraph.trim().slice(0, 220)}` : "."}`;
  }
  if (/\.cs$/i.test(p)) return `Mã C# Unity của Soccer Mobile Pro tại ${p}.`;
  if (/\.unity$/i.test(p)) return `Scene Unity được serialize tại ${p}.`;
  if (/\.prefab$/i.test(p)) return `Prefab Unity được serialize tại ${p}.`;
  return `Cấu hình hoặc công cụ dự án tại ${p}.`;
};

const fileNodes = files.map((p) => {
  const old = oldByPath.get(p);
  return {
    ...(old || {}),
    id: idFor(p),
    type: prefixFor(p),
    name: path.basename(p),
    filePath: p,
    summary: summaryFor(p),
    language: languageFor(p),
    tags: [...new Set([...(old?.tags || []), prefixFor(p), p.split("/")[0].toLowerCase()])],
  };
});
const fileIds = new Map(fileNodes.map((n) => [n.filePath, n.id]));

// Preserve the richer symbol analysis from the previous full run for files that remain in scope.
const symbolNodes = oldGraph.nodes
  .filter((n) => ["class", "function"].includes(n.type) && n.filePath && fileSet.has(n.filePath.replaceAll("\\", "/")))
  .map((n) => ({ ...n, filePath: n.filePath.replaceAll("\\", "/") }));
const nodes = [...fileNodes, ...symbolNodes];
const nodeIds = new Set(nodes.map((n) => n.id));
const edges = oldGraph.edges
  .filter((e) => nodeIds.has(e.source) && nodeIds.has(e.target))
  .map((e) => ({ ...e }));
const edgeKeys = new Set(edges.map((e) => `${e.source}|${e.target}|${e.type}`));
const addEdge = (source, target, type, label, weight = 0.5) => {
  const key = `${source}|${target}|${type}`;
  if (source !== target && nodeIds.has(source) && nodeIds.has(target) && !edgeKeys.has(key)) {
    edges.push({ source, target, type, label, weight });
    edgeKeys.add(key);
  }
};

for (const symbol of symbolNodes) {
  addEdge(fileIds.get(symbol.filePath), symbol.id, "contains", "chứa", 1);
}

// Markdown links make the documentation graph navigable and auditable.
for (const node of fileNodes.filter((n) => n.type === "document")) {
  const content = fs.readFileSync(path.join(root, node.filePath), "utf8");
  for (const match of content.matchAll(/\[[^\]]+\]\(([^)]+)\)/g)) {
    const raw = match[1].split("#")[0];
    if (!raw || /^(https?:|mailto:)/i.test(raw)) continue;
    const targetPath = path.posix.normalize(path.posix.join(path.posix.dirname(node.filePath), decodeURI(raw)));
    const target = fileIds.get(targetPath);
    if (target) addEdge(node.id, target, "documents", "liên kết tài liệu", 0.5);
  }
}

const layerDefs = [
  ["product-docs", "Tài liệu sản phẩm", "GDD, UX, nghiên cứu, system spec, operations và audit triển khai.", (p) => /^(README\.md|AGENTS\.md|docs\/)/.test(p)],
  ["gameplay-code", "Gameplay C#", "Runtime C# điều khiển luồng trận, input, cầu thủ, bóng, AI, luật và HUD.", (p) => /^Assets\/.*\.cs$/i.test(p)],
  ["scenes", "Unity scenes", "Các scene tạo thành navigation và match flow hiện tại.", (p) => /\.unity$/i.test(p)],
  ["prefabs", "Unity prefabs", "Prefab gameplay và presentation được scene tham chiếu.", (p) => /\.prefab$/i.test(p)],
  ["addressables", "Addressables", "Cấu hình phân phối asset và content catalog của Unity.", (p) => /^Assets\/AddressableAssetsData\//.test(p)],
  ["project-config", "Cấu hình dự án", "Build Settings, package, input legacy, quality và version Unity.", (p) => /^(ProjectSettings|Packages)\//.test(p) || /^\.(gitattributes|markdownlint)/.test(p)],
  ["documentation-tools", "Công cụ tài liệu", "Validator Markdown và trình tái tạo knowledge graph.", (p) => /^tools\//.test(p)],
];
const assigned = new Set();
const layers = layerDefs.map(([slug, name, description, test]) => {
  const nodeIdsForLayer = fileNodes.filter((n) => !assigned.has(n.id) && test(n.filePath)).map((n) => (assigned.add(n.id), n.id));
  return { id: `layer:${slug}`, name, description, nodeIds: nodeIdsForLayer };
}).filter((layer) => layer.nodeIds.length);

const tourPaths = [
  ["Bắt đầu từ cổng tài liệu", "Mở chỉ mục để đi tới authority của từng domain.", ["docs/index.md", "README.md"]],
  ["Định hướng sản phẩm", "Đọc GDD và UX contract trước khi thay đổi hành vi.", ["docs/product/gdd-soccer-mobile-pro.md", "docs/product/ux-wireflows-and-states.md"]],
  ["Bằng chứng tham chiếu", "Đối chiếu claim công khai, sổ nguồn, coverage và video evidence.", ["docs/research/fc-mobile-vn-research.md", "docs/research/fc-mobile-vn-source-register.md", "docs/research/fc-mobile-vn-coverage-audit.md", "docs/research/video/ui-pattern-synthesis.md"]],
  ["Hiện trạng triển khai", "Xác nhận phạm vi Unity đã có và backlog P0/P1/P2.", ["docs/implementation/unity-implementation-audit-and-backlog.md"]],
  ["Điều khiển trận đấu", "Theo flow input đến player/ball/AI và presentation.", ["docs/systems/match-controls-set-pieces-and-var.md", "Assets/Scripts/SoccerInput.cs", "Assets/Scripts/Player.cs", "Assets/Scripts/BallScript.cs"]],
  ["Luồng scene", "Theo navigation từ splash/menu tới trận và kết quả.", ["ProjectSettings/EditorBuildSettings.asset"]],
  ["Dữ liệu và live operations", "Hiểu ranh giới authority, entitlement, liveops và integrity.", ["docs/operations/live-data-and-operations.md", "docs/operations/liveops-monetization-and-membership.md", "docs/systems/competitive-integrity-and-esports.md"]],
  ["Kiểm tra tài liệu", "Chạy validator sau khi đổi tên, heading hoặc liên kết.", ["tools/validate-docs.ps1"]],
];
const tour = tourPaths.map(([title, description, paths], index) => ({
  order: index + 1,
  title,
  description,
  nodeIds: paths.map((p) => fileIds.get(p)).filter(Boolean),
}));

const gitCommitHash = execFileSync("git", ["rev-parse", "HEAD"], { cwd: root, encoding: "utf8" }).trim();
const analyzedAt = new Date().toISOString();
const graph = {
  version: "1.0.0",
  project: {
    name: "Soccer Mobile Pro",
    languages: ["C#", "Markdown", "Unity YAML", "PowerShell", "JavaScript", "JSON"],
    frameworks: ["Unity 2022.3.62f3", "Universal Render Pipeline", "Addressables"],
    description: "Dự án game bóng đá mobile Unity; graph tập trung vào runtime, scene/prefab, cấu hình và tài liệu domain đã chuẩn hóa.",
    analyzedAt,
    gitCommitHash,
  },
  nodes,
  edges,
  layers,
  tour,
};

fs.mkdirSync(path.join(uaDir, "intermediate"), { recursive: true });
fs.writeFileSync(graphPath, `${JSON.stringify(graph, null, 2)}\n`, "utf8");
fs.writeFileSync(path.join(uaDir, "meta.json"), `${JSON.stringify({ lastAnalyzedAt: analyzedAt, gitCommitHash, version: "1.0.0", analyzedFiles: files.length }, null, 2)}\n`, "utf8");
fs.writeFileSync(path.join(uaDir, "intermediate", "scan-result.json"), `${JSON.stringify({ project: graph.project, files: files.map((p) => ({ path: p, language: languageFor(p), fileCategory: prefixFor(p) === "document" ? "docs" : prefixFor(p) === "config" ? "config" : "code" })), filteredByIgnore: tracked.length - files.length }, null, 2)}\n`, "utf8");
fs.writeFileSync(path.join(uaDir, "intermediate", "fingerprint-input.json"), `${JSON.stringify({ projectRoot: root, sourceFilePaths: files, gitCommitHash }, null, 2)}\n`, "utf8");
console.log(JSON.stringify({ analyzedFiles: files.length, nodes: nodes.length, edges: edges.length, layers: layers.length, tour: tour.length, gitCommitHash }, null, 2));
