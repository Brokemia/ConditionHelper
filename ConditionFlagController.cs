using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ConditionHelper {
    [CustomEntity("ConditionHelper/ConditionFlagController")]
    public class ConditionFlagController : Entity {
        public string Condition;

        public string Flag;

        public bool RemoveFlag;

        private int watchingID;

        public ConditionFlagController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Condition = data.Attr("condition");
            Flag = data.Attr("flag");
            RemoveFlag = data.Bool("removeFlag");
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            if (string.IsNullOrWhiteSpace(Condition)) {
                RemoveSelf();
                return;
            }
            watchingID = ConditionHelperModule.Instance.ConditionWatcher.WatchConditions(Condition, OnCondition);
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            ConditionHelperModule.Instance.ConditionWatcher.RemoveCallback(watchingID);
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);
            ConditionHelperModule.Instance.ConditionWatcher.RemoveCallback(watchingID);
        }

        private void OnCondition() {
            if (ConditionHelperExports.EvaluateConditionExpression(Condition)) {
                SceneAs<Level>().Session.SetFlag(Flag, !RemoveFlag);
            }
        }
    }
}
