local trigger = {}

trigger.name = "ConditionHelper/ConditionFlagTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        condition = "",
        flag = "",
        removeFlag = false,
        onLeave = false
    }
}

return trigger