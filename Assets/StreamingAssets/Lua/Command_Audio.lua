---------------------------------------------------------------------------------------------------
-- Sound Commands

ambiSoundID = 0

---------------------------------------------------------------------------------------------------
-- Prepare Audio

PrepareChapterAudio = function(chapterName)

    Reserve(function()

        ADV:PrepareChapterAudio(chapterName)

    end)

    WaitTime(0.5)

end


---------------------------------------------------------------------------------------------------
-- BGM

BGMStart = function(file)

	ADV:PlayBGM(file)

end

BGMStop = function()

	ADV:StopBGM()

end

---------------------------------------------------------------------------------------------------
-- Voice

VoiceStart = function(index, file)

	ADV:PlayVoice(file)

end

VoiceStop = function()

	ADV:StopVoice()

end

---------------------------------------------------------------------------------------------------
-- Sound

SoundStart = function(fileName, volume)

	volume = volume or 0.7

	--local formattedIndex = string.format("%03d", fileIndex)
	--local fileName = "SE/se_".. formattedfileName ..".mp3"
	local option = { Loop = false, Volume = volume }

	ADV:StopSound(ambiSoundID)
	ambiSoundID = ADV:PlaySound(fileName, option)

end

SoundStop = function()

	ADV:StopSound(ambiSoundID)

end

