---------------------------------------------------------------------------------------------------
-- Fade In
FadeInStart = function(duration)

    duration = duration or 0.5
    Update()

    ADV:FadeIn(duration)
    PauseCoroutine()

end

---------------------------------------------------------------------------------------------------
-- Fade Out

FadeOutStart = function(duration)

    duration = duration or 0.5
    Update()

    ADV:FadeOut(duration)
    PauseCoroutine()

end

---------------------------------------------------------------------------------------------------
-- set background shake 

SetBackgroundShake = function(shakeDuration, shakeStrengthX, shakeStrengthY, shakeCount)

    local function Vector2(x, y)
        return {
            x = x,
            y = y
        }
    end

    shakeStrengthX = shakeStrengthX or 40
    shakeStrengthY = shakeStrengthY or 40
    local shakeStrength = Vector2(shakeStrengthX, shakeStrengthY)

    shakeDuration = shakeDuration or 0.03
    shakeCount = shakeCount or 3

    Reserve(function()

        ADV:SetUniversalShake(bgObjectID, "x", shakeStrength, shakeDuration, 0, shakeCount, 6)
        ADV:SetUniversalShake(bgObjectID, "y", shakeStrength, shakeDuration, shakeDuration / 4, shakeCount, 6)

        for id, charaId in pairs(charaObjectIDTable) do

            ADV:SetUniversalShake(charaId, "x", shakeStrength, shakeDuration, 0, shakeCount, 6)
            ADV:SetUniversalShake(charaId, "y", shakeStrength, shakeDuration, shakeDuration / 4, shakeCount, 6)

        end

    end)

end

---------------------------------------------------------------------------------------------------
-- set text window shake 

SetTextWindowShake = function(shakeDuration, shakeStrengthX, shakeStrengthY, shakeCount)

    local function Vector2(x, y)
        return {
            x = x,
            y = y
        }
    end

    shakeStrengthX = shakeStrengthX or 40
    shakeStrengthY = shakeStrengthY or 40
    local shakeStrength = Vector2(shakeStrengthX, shakeStrengthY)

    shakeDuration = shakeDuration or 0.03
    shakeCount = shakeCount or 3

    Reserve(function()

        ADV:SetTextWindowShake("x", shakeStrength, shakeDuration, 0, shakeCount, 6)
        ADV:SetTextWindowShake("y", shakeStrength, shakeDuration, shakeDuration / 4, shakeCount, 6)

    end)

end

