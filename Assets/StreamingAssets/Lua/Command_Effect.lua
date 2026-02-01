---------------------------------------------------------------------------------------------------
-- Flash 

FlashStart = function(flashColor, waitDuration, endDuration)

    waitDuration = waitDuration or 0
    endDuration = endDuration or 5

    if flashColor ~= nil and flashColor ~= "" then

        local firstChar = string.sub(flashColor, 1, 1) 
        local remainingString = string.sub(flashColor, 2) 

        if not firstChar:match("%u") then
            firstChar = firstChar:upper()
        end

        remainingString = remainingString:lower()
        flashColor = firstChar .. remainingString

    end

    local flashSpineFile = "Effect/effect_" .. flashColor .. ".png"

    ADV:PrepareAssetSprite(flashSpineFile)

    Reserve(function()

        ADV:ObjectCreateSprite(effectObjectIDTable[0], flashSpineFile, {
            Level = OverLevelID
        })
        ADV:SetFlash(effectObjectIDTable[0], waitDuration, endDuration)

    end)
end

---------------------------------------------------------------------------------------------------
-- Create / Delete Icon

IconCreate = function(iconFile)

    iconFile = "icon_" .. iconFile .. ".png"

    ADV:PrepareAssetSprite(iconFile)

    Reserve(function()

        ADV:ObjectCreateSprite(effectObjectIDTable[1], iconFile, {
            Level = OverLevelID,
            FadeTime = 0.5
        })

        ADV:WaitTime(0.6)
        PauseCoroutine()

    end)

    Update()

end

IconDelete = function()

    Reserve(function()

        ADV:ObjectDelete(effectObjectIDTable[1], {
            FadeTime = 0.5
        })

        ADV:WaitTime(0.6)
        PauseCoroutine()

    end)

    Update()

end
