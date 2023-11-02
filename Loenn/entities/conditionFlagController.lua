local conditionFlagController = {}

conditionFlagController.name = "ConditionHelper/ConditionFlagController"
conditionFlagController.depth = 0
conditionFlagController.texture = "Loenn/ConditionHelper/ConditionFlagController"
conditionFlagController.placements = {
    name = "controller",
    data = {
        condition = "",
        flag = "",
        removeFlag = false
    }
}

return conditionFlagController