using System.Collections.Generic;
using CnP.Core;
using CnP.Domain.Unit;
using UnityEngine;

namespace CnP.Flow
{
    /// <summary>
    /// 棋盘系统：单位注册表 + 自动入位（§7.2.1 职能分道）+ 部署上限（#D14 固定 36）。
    /// 支援区后置：满员后禁止继续召唤。
    /// </summary>
    public class BoardSystem
    {
        readonly List<UnitInstance> _units = new List<UnitInstance>();
        int _nextId = 1;

        // 职能车道 X 坐标：坦克靠前（近中线）、战士居中、射手/辅助靠后
        const float LaneTank = -2.1f;
        const float LaneWarrior = -4.4f;
        const float LaneBack = -6.7f;
        float _spawnCursorY; // 纵向轮流铺开

        public IReadOnlyList<UnitInstance> Units => _units;
        public int PlayerUnitCount { get; private set; }

        /// <summary>是否还能部署 count 个单位（上限 36 含英雄/将领，本切片无英雄）</summary>
        public bool CanDeploy(int count)
        {
            return PlayerUnitCount + count <= GameParams.DeployLimit;
        }

        /// <summary>按召唤订单批量生成玩家单位（自动入位，后续可点击微调）</summary>
        public List<UnitInstance> SpawnPlayerUnits(List<SummonBatch> batches)
        {
            var spawned = new List<UnitInstance>();
            foreach (var batch in batches)
            {
                for (int i = 0; i < batch.Count; i++)
                {
                    var inst = new UnitInstance
                    {
                        Id = _nextId++,
                        Side = Side.Player,
                        Stats = batch.Template,
                        Hp = batch.Template.Hp,
                        MaxHp = batch.Template.Hp,
                    };
                    inst.Position = NextSpawnPosition(batch.Template.Role);
                    _units.Add(inst);
                    PlayerUnitCount++;
                    spawned.Add(inst);
                }
            }
            FlowEvents.RaiseBoardChanged();
            return spawned;
        }

        /// <summary>生成敌方单位（S5 敌军生成器调用，按指定纵队排布）</summary>
        public UnitInstance SpawnEnemyUnit(UnitStats stats, Vector2 position)
        {
            var inst = new UnitInstance
            {
                Id = _nextId++,
                Side = Side.Enemy,
                Stats = stats,
                Hp = stats.Hp,
                MaxHp = stats.Hp,
                Position = BoardGeometry.ClampToEnemyZone(position),
            };
            _units.Add(inst);
            FlowEvents.RaiseBoardChanged();
            return inst;
        }

        /// <summary>移动己方单位（钳制玩家半场）</summary>
        public void MovePlayerUnit(UnitInstance unit, Vector2 target)
        {
            if (unit == null || unit.Side != Side.Player) return;
            unit.Position = BoardGeometry.ClampToPlayerZone(target);
        }

        // ── 多选与编组（§7.2.3 RTS 式布阵）──────────────

        readonly HashSet<int> _selectedIds = new HashSet<int>();

        /// <summary>当前选中单位 Id 集合（仅己方）</summary>
        public IReadOnlyCollection<int> SelectedIds => _selectedIds;

        public void SelectSingle(UnitInstance unit)
        {
            _selectedIds.Clear();
            if (unit != null && unit.Side == Side.Player && unit.Alive)
                _selectedIds.Add(unit.Id);
        }

        public void ToggleSelect(UnitInstance unit)
        {
            if (unit == null || unit.Side != Side.Player || !unit.Alive) return;
            if (!_selectedIds.Add(unit.Id)) _selectedIds.Remove(unit.Id);
        }

        /// <summary>全选同兵种名单位（双击同类全选）</summary>
        public void SelectAllOfType(string unitName)
        {
            _selectedIds.Clear();
            foreach (var u in _units)
                if (u.Side == Side.Player && u.Alive && u.Stats.Name == unitName)
                    _selectedIds.Add(u.Id);
        }

        /// <summary>全选场上己方单位（Ctrl+A）</summary>
        public void SelectAllPlayers()
        {
            _selectedIds.Clear();
            foreach (var u in _units)
                if (u.Side == Side.Player && u.Alive)
                    _selectedIds.Add(u.Id);
        }

        public void ClearSelection() => _selectedIds.Clear();

        /// <summary>框选：把矩形内的己方单位加入选中（additive=false 时先清空）</summary>
        public void SelectInRect(Vector2 a, Vector2 b, bool additive)
        {
            if (!additive) _selectedIds.Clear();
            var min = Vector2.Min(a, b);
            var max = Vector2.Max(a, b);
            foreach (var u in _units)
            {
                if (u.Side != Side.Player || !u.Alive) continue;
                if (u.Position.x >= min.x && u.Position.x <= max.x &&
                    u.Position.y >= min.y && u.Position.y <= max.y)
                    _selectedIds.Add(u.Id);
            }
        }

        /// <summary>取选中单位（稳定按 Id 升序，保证线列落位顺序可预期）</summary>
        public List<UnitInstance> GetSelected()
        {
            var list = new List<UnitInstance>();
            foreach (var u in _units)
                if (_selectedIds.Contains(u.Id) && u.Alive) list.Add(u);
            list.Sort((x, y) => x.Id.CompareTo(y.Id));
            return list;
        }

        /// <summary>整组保持相对阵型移动到目标点（右键单击；逐单位钳制玩家半场）</summary>
        public void MoveSelectedGroupTo(Vector2 target)
        {
            var sel = GetSelected();
            if (sel.Count == 0) return;
            var current = new Vector2[sel.Count];
            for (int i = 0; i < sel.Count; i++) current[i] = sel[i].Position;
            var targets = FormationPlanner.GroupMove(current, target);
            for (int i = 0; i < sel.Count; i++)
                sel[i].Position = BoardGeometry.ClampToPlayerZone(targets[i]);
        }

        /// <summary>线列落位（Shift+右键拖拽松开；anchor=线列中心，halfLength=拖拽半长）</summary>
        public void PlaceSelectedInLine(Vector2 anchor, float halfLength)
        {
            var sel = GetSelected();
            if (sel.Count == 0) return;
            var arranged = FormationPlanner.ArrangeLine(
                anchor, sel.Count, halfLength,
                BoardGeometry.PlayerZoneWidth(), BoardGeometry.PlayerZoneHeight());
            for (int i = 0; i < sel.Count; i++)
                sel[i].Position = BoardGeometry.ClampToPlayerZone(arranged[i]);
        }

        public void Clear()
        {
            _units.Clear();
            PlayerUnitCount = 0;
            _spawnCursorY = 0f;
            _selectedIds.Clear();
            FlowEvents.RaiseBoardChanged();
        }

        /// <summary>按职能取初始车道位置；纵向从中心向两侧轮流展开</summary>
        Vector2 NextSpawnPosition(UnitRole role)
        {
            float x = role == UnitRole.坦克 ? LaneTank
                    : role == UnitRole.战士 ? LaneWarrior
                    : LaneBack;

            int index = PlayerUnitCount;
            int row = index / 7;                         // 每列 7 个后换下一列（更靠左）
            float y = -2.6f + (index % 7) * 0.87f + _spawnCursorY * 0f;
            float finalX = x - row * 0.95f;
            return BoardGeometry.ClampToPlayerZone(new Vector2(finalX, y));
        }
    }
}
