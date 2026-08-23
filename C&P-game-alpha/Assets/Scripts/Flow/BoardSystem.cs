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

        public void Clear()
        {
            _units.Clear();
            PlayerUnitCount = 0;
            _spawnCursorY = 0f;
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
