using System.Collections.Generic;
using CnP.Core;
using CnP.Domain.Unit;
using CnP.Flow;
using UnityEngine;

namespace CnP.UI
{
    /// <summary>
    /// 棋盘渲染 + 出牌阶段点击交互（选中己方单位 → 点空位移动，简化版站位调整）。
    /// 表现 = 职能色块 + 兵种名；血条在 S4 战斗引擎接入。
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        static readonly Dictionary<UnitRole, Color> RoleColors = new Dictionary<UnitRole, Color>
        {
            { UnitRole.战士, new Color(0.76f, 0.34f, 0.30f) },
            { UnitRole.坦克, new Color(0.30f, 0.47f, 0.77f) },
            { UnitRole.射手, new Color(0.33f, 0.66f, 0.38f) },
            { UnitRole.辅助, new Color(0.79f, 0.64f, 0.31f) },
        };
        static readonly Color EnemyTint = new Color(0.55f, 0.32f, 0.55f);

        Transform _root;
        readonly Dictionary<int, GameObject> _views = new Dictionary<int, GameObject>();
        readonly Dictionary<int, SpriteRenderer> _bodies = new Dictionary<int, SpriteRenderer>();
        readonly Dictionary<int, Transform> _hpFills = new Dictionary<int, Transform>();
        public int SelectedUnitId { get; private set; }

        const float BarWidth = 0.7f;
        const float BarY = 0.42f;

        Font _font;

        void Awake()
        {
            _root = new GameObject("Units").transform;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            FlowEvents.BoardChanged += Rebuild;
        }

        void OnDestroy()
        {
            FlowEvents.BoardChanged -= Rebuild;
        }

        void Update()
        {
            var flow = RoundFlowController.Instance;
            if (flow == null) return;
            SyncViews();
            HandleInput(flow);
        }

        /// <summary>棋盘集合变化时全量重建视图（数量小，代价可忽略）</summary>
        void Rebuild()
        {
            foreach (var go in _views.Values) Destroy(go);
            _views.Clear();
            _bodies.Clear();
            _hpFills.Clear();

            var flow = RoundFlowController.Instance;
            if (flow == null) return;
            foreach (var unit in flow.Board.Units)
                CreateView(unit);
        }

        void CreateView(UnitInstance unit)
        {
            var root = new GameObject("Unit_" + unit.Id + "_" + unit.Stats.Name);
            root.transform.SetParent(_root, false);
            root.transform.position = new Vector3(unit.Position.x, unit.Position.y, 0f);

            var body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            var sr = body.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeAssets.WhiteSquare;
            sr.sortingOrder = 10;
            sr.color = ColorFor(unit);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(root.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 0.56f, 0f);
            var tm = labelGo.AddComponent<TextMesh>();
            tm.font = _font;
            tm.text = unit.Stats.Name;
            tm.fontSize = 30;
            tm.characterSize = 0.12f;
            tm.anchor = TextAnchor.LowerCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.88f, 0.88f, 0.92f);
            var mr = labelGo.GetComponent<MeshRenderer>();
            mr.sharedMaterial = _font.material;
            mr.sortingOrder = 12;

            // 血条（底 + 填充，填充按 Hp/MaxHp 左锚伸缩）
            var hpBg = MakeBar(root.transform, "HpBg", new Color(0.08f, 0.08f, 0.1f), 11);
            hpBg.localPosition = new Vector3(0f, BarY, 0f);
            var hpFill = MakeBar(root.transform, "HpFill", Color.green, 12);
            hpFill.localPosition = new Vector3(-BarWidth * 0.5f + BarWidth * 0.5f, BarY, 0f); // SyncViews 每帧校准
            _hpFills[unit.Id] = hpFill;

            _views[unit.Id] = root;
            _bodies[unit.Id] = sr;
        }

        static Transform MakeBar(Transform parent, string name, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(BarWidth, 0.085f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeAssets.WhiteSquare;
            sr.color = color;
            sr.sortingOrder = order;
            return go.transform;
        }

        /// <summary>每帧同步位置/选中态/血条（战斗移动由 CombatSystem 写 Position）</summary>
        void SyncViews()
        {
            var flow = RoundFlowController.Instance;
            foreach (var unit in flow.Board.Units)
            {
                if (!_views.TryGetValue(unit.Id, out var go)) continue;
                go.transform.position = new Vector3(unit.Position.x, unit.Position.y, 0f);
                if (_bodies.TryGetValue(unit.Id, out var sr))
                {
                    bool selected = unit.Id == SelectedUnitId && unit.Side == Side.Player;
                    var baseColor = ColorFor(unit);
                    sr.color = selected ? Color.Lerp(baseColor, Color.white, 0.45f) : baseColor;
                    go.transform.GetChild(0).localScale = selected
                        ? new Vector3(0.86f, 0.86f, 1f)
                        : new Vector3(0.72f, 0.72f, 1f);
                }
                if (_hpFills.TryGetValue(unit.Id, out var fill))
                {
                    float f = unit.MaxHp > 0f ? Mathf.Clamp01(unit.Hp / unit.MaxHp) : 0f;
                    fill.localScale = new Vector3(BarWidth * f, 0.085f, 1f);
                    fill.localPosition = new Vector3(-BarWidth * 0.5f + BarWidth * f * 0.5f, BarY, 0f);
                    fill.GetComponent<SpriteRenderer>().color = Color.Lerp(
                        new Color(0.8f, 0.28f, 0.22f), new Color(0.32f, 0.78f, 0.38f), f);
                }
            }
        }

        /// <summary>出牌阶段点击：点己方单位 = 选中；选中后点玩家半场空位 = 移动</summary>
        void HandleInput(RoundFlowController flow)
        {
            if (flow.Phase != Phase.Play || !Input.GetMouseButtonDown(0)) return;

            var cam = Camera.main;
            if (cam == null) return;
            var wp = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -cam.transform.position.z));

            // 命中己方单位（半径 0.45）
            UnitInstance hit = null;
            foreach (var u in flow.Board.Units)
            {
                if (u.Side != Side.Player) continue;
                if (Vector2.Distance(wp, u.Position) <= 0.45f) { hit = u; break; }
            }

            if (hit != null)
            {
                SelectedUnitId = hit.Id;
                return;
            }

            if (SelectedUnitId != 0)
            {
                UnitInstance selected = null;
                foreach (var u in flow.Board.Units)
                    if (u.Id == SelectedUnitId) { selected = u; break; }

                if (selected != null && wp.x < 0f) // 只允许移到玩家半场
                {
                    flow.Board.MovePlayerUnit(selected, wp);
                }
                SelectedUnitId = 0;
            }
        }

        Color ColorFor(UnitInstance unit)
        {
            var baseColor = RoleColors[unit.Stats.Role];
            return unit.Side == Side.Enemy
                ? Color.Lerp(baseColor, EnemyTint, 0.65f)
                : baseColor;
        }
    }
}
