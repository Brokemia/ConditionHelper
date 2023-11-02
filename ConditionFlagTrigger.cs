using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ConditionHelper {
    [CustomEntity("ConditionHelper/ConditionFlagTrigger")]
    public class ConditionFlagTrigger : Trigger {
        public string Condition;

        public string Flag;

        public bool RemoveFlag;

        public bool IsOnLeave;

        public ConditionFlagTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            Condition = data.Attr("condition");
            Flag = data.Attr("flag");
            RemoveFlag = data.Bool("removeFlag");
            IsOnLeave = data.Bool("onLeave");
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (string.IsNullOrWhiteSpace(Condition)) {
                RemoveSelf();
            }
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            if (!IsOnLeave && ConditionHelperExports.EvaluateConditionExpression(Condition)) {
                SceneAs<Level>().Session.SetFlag(Flag, !RemoveFlag);
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);
            if (IsOnLeave && ConditionHelperExports.EvaluateConditionExpression(Condition)) {
                SceneAs<Level>().Session.SetFlag(Flag, !RemoveFlag);
            }
        }
    }
}
