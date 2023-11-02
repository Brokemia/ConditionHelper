using MonoMod.ModInterop;
using Soukoku.ExpressionParser;
using System;

namespace Celeste.Mod.ConditionHelper {
    [ModExportName("ConditionHelper")]
    public static class ConditionHelperExports {
        /// <summary>
        /// Let registered callbacks know that the value of a given condition may have changed
        /// </summary>
        /// <param name="condition">Which condition changed</param>
        public static void ConditionChanged(string condition) {
            ConditionHelperModule.Instance.ConditionWatcher.DoCallbacks(condition);
        }

        /// <summary>
        /// Register a callback for whenever a given condition in the expression may have changed.<br/>
        /// The callback should still check the expression, getting called does not guarantee a change in value.
        /// </summary>
        /// <param name="expression">The expression to watch conditions of</param>
        /// <param name="callback">The callback to be called when a condition changes</param>
        /// <returns>An ID to uniquely identify this callback. Useful if you need to remove it later</returns>
        public static int WatchConditions(string expression, Action callback) {
            return ConditionHelperModule.Instance.ConditionWatcher.WatchConditions(expression, callback);
        }

        /// <summary>
        /// Unregister a callback from watching a condition.
        /// </summary>
        /// <param name="callbackID">The ID of the callback to remove</param>
        public static void RemoveCallback(int callbackID) {
            ConditionHelperModule.Instance.ConditionWatcher.RemoveCallback(callbackID);
        }

        /// <summary>
        /// Evaluates the boolean value of an expression using conditions
        /// </summary>
        /// <param name="expression">The expression to evaluate</param>
        /// <returns>The result of the expression</returns>
        public static bool EvaluateConditionExpression(string expression) {
            return ConditionHelperModule.Instance.ExpressionEvaluator.Evaluate(expression, true).Equals(ExpressionToken.True);
        }
    }
}
