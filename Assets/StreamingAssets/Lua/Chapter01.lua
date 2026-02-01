function ADV_Start()

    StartCoroutine(Main_Story) 

end

function Main_Story()

    PrepareChapterAudio("Chapter01")


    BGMStart("BGM/bgm_chapter01.mp3")

    FadeOutStart()

    SetBackground("Background/bg_002.png")

    SetCharacter(1, 2)
    SetCharacter(2, 3)
    SetTextWindowOpen()
    FadeInStart()


    SetText(1000001, 1, "你好吗")
    SetCharacterJump(1)
    SetText(1000002, 2, "我很好，谢谢!, 你呢？")
    SetCharacterShake(2)
    SoundStart("SE/se_car.mp3")
    SetText(1000003, 1, "我也很好，谢谢!")

end

