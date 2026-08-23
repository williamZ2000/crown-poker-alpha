using CnP.Core;
using NUnit.Framework;
using UnityEngine;

namespace CnP.Tests
{
    /// <summary>布阵算法单测（§7.2.3：线列排布/折行/间距压缩/整组移动）</summary>
    public class FormationPlannerTests
    {
        const float W = 7.3f; // 玩家半场可用宽（近似值，测试用）
        const float H = 6.0f; // 玩家半场可用深

        [Test]
        public void 线列_单行居中默认间距()
        {
            var anchor = new Vector2(-4f, 1f);
            var pts = FormationPlanner.ArrangeLine(anchor, 4, 0f, W, H);
            Assert.AreEqual(4, pts.Length);
            foreach (var p in pts) Assert.AreEqual(anchor.y, p.y, 0.0001f); // 单行同 y
            // 以 anchor.x 为中心对称，间距 = 默认 0.85
            Assert.AreEqual(anchor.x - 1.275f, pts[0].x, 0.001f);
            Assert.AreEqual(anchor.x + 1.275f, pts[3].x, 0.001f);
            Assert.AreEqual(FormationPlanner.DefaultSpacing, pts[1].x - pts[0].x, 0.001f);
        }

        [Test]
        public void 线列_间距随拖拽长度缩放并有上下限()
        {
            var anchor = Vector2.zero;
            // 短拖：span=1.2, 1.2/3=0.4 → 钳到下限 0.55
            var tight = FormationPlanner.ArrangeLine(anchor, 4, 0.6f, W, H);
            Assert.AreEqual(FormationPlanner.MinSpacing, tight[1].x - tight[0].x, 0.001f);
            // 长拖：span=6, 6/3=2 → 钳到上限 0.85
            var loose = FormationPlanner.ArrangeLine(anchor, 4, 3f, W, H);
            Assert.AreEqual(FormationPlanner.DefaultSpacing, loose[1].x - loose[0].x, 0.001f);
            // 适中拖：span=2.4, 2.4/3=0.8 → 恰好用拖拽值
            var mid = FormationPlanner.ArrangeLine(anchor, 4, 1.2f, W, H);
            Assert.AreEqual(0.8f, mid[1].x - mid[0].x, 0.001f);
        }

        [Test]
        public void 线列_放不下自动折行()
        {
            // W=7.3 默认间距单行容量 = floor(7.3/0.85)+1 = 9 → 10 个单位折 2 行
            var anchor = new Vector2(-4f, -2f);
            var pts = FormationPlanner.ArrangeLine(anchor, 10, 0f, W, H);
            int row0 = 0, row1 = 0;
            foreach (var p in pts)
            {
                if (Mathf.Abs(p.y - anchor.y) < 0.0001f) row0++;
                else if (Mathf.Abs(p.y - (anchor.y + FormationPlanner.RowSpacing)) < 0.0001f) row1++;
            }
            Assert.AreEqual(9, row0);
            Assert.AreEqual(1, row1);
        }

        [Test]
        public void 线列_多行连续折行()
        {
            // W=2.5 → 单行 3 个 → 20 个单位折 7 行（3×6+2）
            var anchor = Vector2.zero;
            var pts = FormationPlanner.ArrangeLine(anchor, 20, 0f, 2.5f, 100f);
            var rows = new System.Collections.Generic.HashSet<float>();
            foreach (var p in pts) rows.Add(Mathf.Round(p.y * 1000f) / 1000f);
            Assert.AreEqual(7, rows.Count);
            // 每行不超过 3 个
            foreach (var r in rows)
            {
                int c = 0;
                foreach (var p in pts) if (Mathf.Abs(p.y - r) < 0.001f) c++;
                Assert.LessOrEqual(c, 3);
            }
        }

        [Test]
        public void 线列_深度不足压缩行距兜底()
        {
            // 4 行 × 默认行距需 3×0.85=2.55，可用深度只有 1.2 → 行距压到 0.4
            var anchor = Vector2.zero;
            var pts = FormationPlanner.ArrangeLine(anchor, 12, 0f, 2.5f, 1.2f); // 单行3个 → 4行
            var ys = new System.Collections.Generic.List<float>();
            foreach (var p in pts)
                if (!ys.Exists(v => Mathf.Abs(v - p.y) < 0.001f)) ys.Add(p.y);
            ys.Sort();
            Assert.AreEqual(4, ys.Count);
            for (int i = 1; i < ys.Count; i++)
                Assert.AreEqual(0.4f, ys[i] - ys[i - 1], 0.01f); // 1.2/3 = 0.4
        }

        [Test]
        public void 线列_边界数量()
        {
            Assert.AreEqual(0, FormationPlanner.ArrangeLine(Vector2.zero, 0, 1f, W, H).Length);
            var single = FormationPlanner.ArrangeLine(new Vector2(1f, 2f), 1, 1f, W, H);
            Assert.AreEqual(1, single.Length);
            Assert.AreEqual(new Vector2(1f, 2f), single[0]);
        }

        [Test]
        public void 整组移动_保持相对阵型()
        {
            var current = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0.5f),
                new Vector2(-2f, 1f),
            };
            var target = new Vector2(-6f, 2f);
            var moved = FormationPlanner.GroupMove(current, target);

            // 质心对齐目标点
            var centroid = Vector2.zero;
            foreach (var p in moved) centroid += p;
            centroid /= moved.Length;
            Assert.AreEqual(target, centroid);

            // 相对偏移保持不变
            for (int i = 1; i < current.Length; i++)
            {
                var before = current[i] - current[0];
                var after = moved[i] - moved[0];
                Assert.AreEqual(before, after);
            }
        }
    }
}
