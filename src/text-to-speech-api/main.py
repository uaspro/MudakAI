from fastapi import FastAPI, Response, Body
from ukrainian_tts.tts import TTS, Voices, Stress
from pydantic import BaseModel
import io

app = FastAPI()

ukr_tts = TTS(device="cpu")  # can try cpu, cuda

class TTSRequest(BaseModel):
    text: str
    voice: str = "dmytro"

@app.post("/tts/")
async def generate_wav(request: TTSRequest):
    # Use an in-memory file to store the WAV data
    with io.BytesIO() as temp_file:
        _, output_text = ukr_tts.tts(request.text, request.voice, Stress.Dictionary.value, temp_file)
        wav_data = temp_file.getvalue()
    
    return Response(content=wav_data, media_type="audio/wav")