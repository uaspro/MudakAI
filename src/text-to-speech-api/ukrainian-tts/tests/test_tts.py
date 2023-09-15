from ukrainian_tts.tts import TTS, Voices, Stress
from io import BytesIO


def test_tts():
    tts = TTS()
    file = BytesIO()
    _, text = tts.tts("Привіт", Voices.Dmytro.value, Stress.Dictionary.value, file)
    file.seek(0)
    assert text == "прив+іт"
    assert file.getbuffer().nbytes > 1000  # check that file was generated
