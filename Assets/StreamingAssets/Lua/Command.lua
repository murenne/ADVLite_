--------------------------------------------------------------------------------------------------
-- Constant

BackgroundLevelID = 1
CharacterLevelID = 2
OverLevelID = 3

CharacterOrder = 1

reservedFuncTable = {}

---------------------------------------------------------------------------------------------------
-- Character

CharaTemplate = {}
CharaTemplate.new = function()
    local obj = {}
    obj.File = ""
    obj.Pos = 0
    return obj
end

charaInfoTable = {}

charaPosXTable = {}
charaPosXTable[1] = 0
charaPosXTable[2] = -300
charaPosXTable[3] = 300
charaPosXTable[4] = -450
charaPosXTable[5] = 450

---------------------------------------------------------------------------------------------------
-- ID

bgObjectID = 1

charaObjectIDTable = {}
charaObjectIDTable[1] = 1001
charaObjectIDTable[2] = 1002
charaObjectIDTable[3] = 1003
charaObjectIDTable[4] = 1004
charaObjectIDTable[5] = 1005

effectObjectIDTable = {}
effectObjectIDTable[0] = 120
effectObjectIDTable[1] = 121

---------------------------------------------------------------------------------------------------
-- Wait Time

WaitTime = function(duration)

    Update()

    ADV:WaitTime(duration)
    PauseCoroutine()

end

---------------------------------------------------------------------------------------------------
-- Wait Key

WaitKey = function()

    ADV:WaitKey()
    PauseCoroutine()

end

---------------------------------------------------------------------------------------------------
-- Set Text

SetText = function(textId, charaId, text, option)
    option = option or {}

    Reserve(function()
        if charaId > 0 then
            ADV:ObjectSetTargetChara(charaObjectIDTable[charaId])
        end
    end)

    Update()

    if textId > 0 then
        local voiceFile = string.format("Voice/voice_"..textId..".mp3")
        VoiceStart(charaId, voiceFile)
    end

    --ADV:TextWindowOpen()

    ADV:SetText(textId, charaId, text)
    PauseCoroutine()

    if option.NoWaitKey == 1 then
        ADV:WaitTime(0.1)
    else
        ADV:WaitKey()
    end
    PauseCoroutine()

    VoiceStop()
end

---------------------------------------------------------------------------------------------------
-- Text window open/close

SetTextWindowOpen = function()

    ADV:TextWindowOpen()
    PauseCoroutine()

end

SetTextWindowClose = function()

    ADV:TextWindowClose()
    PauseCoroutine()

end

---------------------------------------------------------------------------------------------------
-- Background

SetBackground = function(file)

    isSafeView = string.sub(file, 1, 3) == "cg_"

    ADV:PrepareAssetSprite(file)

    Reserve(function()

        ADV:ObjectCreateSprite(bgObjectID, file, {
            Level = BackgroundLevelID,
            IsSafeView = isSafeView
        })

    end)
end

---------------------------------------------------------------------------------------------------
-- Set Character 

SetCharacter = function(charaId, pos, fadeTime, option)

    fadeTime = fadeTime or 0.5

    option = option or {}
    option.Pos = pos
    option.FadeTime = fadeTime

    local formattedIndex = string.format("%03d", charaId) 
    local spineFileName = formattedIndex.."/" .. formattedIndex .. "_SkeletonData.asset"

    CharacterOrder = CharacterOrder + 1

    charaInfoTable[charaId] = charaInfoTable[charaId] or CharaTemplate.new()
    charaInfoTable[charaId].File = spineFileName
    charaInfoTable[charaId].Pos = option.Pos or charaInfoTable[charaId].Pos

    local pos = charaInfoTable[charaId].Pos

    if pos == 0 then
        error("position is 0" .. spineFileName)
    end

    local posX = charaPosXTable[pos]

    -- Merge table
    option = MergeTables({
        Level = CharacterLevelID,
        PosX = posX,
        RenderPosY = 0,
        RenderScale = 1,
        Order = CharacterOrder
    }, option)

    ADV:PrepareAssetSpine(spineFileName)

    Reserve(function()

        -- Create sprite object
        ADV:ObjectCreateSpine(charaObjectIDTable[charaId], spineFileName, option)

        ---ADV:ObjectSetSpineBreath(charaObjectIDTable[charaId]) -- breath & idle motion in track 0
        -- ADV:ObjectSetSpineEye(charaObjectIDTable[charaId], 1) -- eye in track 2
        -- ADV:ObjectSetSpineLipSync(charaObjectIDTable[charaId], 1) -- lip in track 3

        if charaId > 0 then
            ADV:ObjectSetTargetChara(charaObjectIDTable[charaId])
        end

    end)

end

SetCharacterBody = function(charaId, bodyMotionId)

    -- tranc 1

    Reserve(function()

        ADV:ObjectSetSpineBody(charaObjectIDTable[charaId], bodyMotionId)

    end)
end

SetCharacterEye = function(charaId, eyeMotionId)

    -- tranc 2

    Reserve(function()

        ADV:ObjectSetSpineEye(charaObjectIDTable[charaId], eyeMotionId)

    end)
end

SetCharacterLip = function(charaId, lipMotionId)

    -- tranc 3

    Reserve(function()

        ADV:ObjectSetSpineLipSync(charaObjectIDTable[charaId], lipMotionId)

    end)
end

SetCharacterPos = function(charaId, pos)

    charaInfoTable[charaId].Pos = pos

end

SetTargetCharacter = function(charaId)

    Reserve(function()

        ADV:ObjectSetTargetChara(charaObjectIDTable[charaId])

    end)

end

---------------------------------------------------------------------------------------------------
-- Delete Character

ClearCharacter = function(charaId, fadeTime)

    local fadeTime = fadeTime or 0.5

    charaInfoTable[charaId] = nil

    Reserve(function()

        ADV:ObjectDelete(charaObjectIDTable[charaId], {
            FadeTime = fadeTime
        })

    end)
end

ClearAllCharacters = function(fadeTime)

    local fadeTime = fadeTime or 0.5

    for charaId, objId in pairs(charaObjectIDTable) do

        charaInfoTable[charaId] = nil

    end

    Reserve(function()

        for charaId, objId in pairs(charaObjectIDTable) do

            ADV:ObjectDelete(charaObjectIDTable[charaId], {
                FadeTime = fadeTime
            })

        end
    end)
end

---------------------------------------------------------------------------------------------------
-- Update

Update = function()

    ADV:WaitTask()
    
    PauseCoroutine()
    ReservedInvoke()

    ADV:UpdateView()
    ADV:ReleaseAllPreparedAsset()

end

---------------------------------------------------------------------------------------------------
-- Reserve

Reserve = function(func, ...)

    table.insert(reservedFuncTable, {
        func = func,
        args = {...}
    })

end

ReservedInvoke = function(func, ...)

    for _, entry in ipairs(reservedFuncTable) do
        entry.func(table.unpack(entry.args))
    end

    reservedFuncTable = {}

end

