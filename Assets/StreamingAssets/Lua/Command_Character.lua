---------------------------------------------------------------------------------------------------
-- jump

SetCharacterJump = function(id, duration)

    duration = duration or 0.2

    ADV:ObjectMovePosY(charaObjectIDTable[id], 100, duration, 6)
    WaitTime(duration)
    ADV:ObjectMovePosY(charaObjectIDTable[id], -100, duration, 5)

end

--------------------------------------------------------------------------------------------------
-- move up / down

SetCharacterMoveVertical = function(id, duration, distance)

    duration = duration or 0.2

    ADV:ObjectMovePosY(charaObjectIDTable[id], distance, duration, 6)

end

--------------------------------------------------------------------------------------------------
-- shake

SetCharacterShake = function(id, shakeDuration, shakeStrengthX, shakeStrengthY, shakeCount)

    local function Vector2(x, y)
        return {
            x = x,
            y = y
        }
    end

    shakeStrengthX = shakeStrengthX or 40
    shakeStrengthY = shakeStrengthY or 0
    local shakeStrength = Vector2(shakeStrengthX, shakeStrengthY)

    shakeDuration = shakeDuration or 0.03
    shakeCount = shakeCount or 3

    Reserve(function()

        ADV:SetUniversalShake(charaObjectIDTable[id], "x", shakeStrength, shakeDuration, 0, shakeCount, 6)
        ADV:SetUniversalShake(charaObjectIDTable[id], "y", shakeStrength, shakeDuration, shakeDuration / 4,shakeCount, 6)

    end)
end


