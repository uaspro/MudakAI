# syntax=docker/dockerfile:1

FROM python:3.10

WORKDIR /code

COPY num2words/ ./num2words/
COPY ukrainian-accentor/ ./ukrainian-accentor/
COPY ukrainian-tts/ ./ukrainian-tts/
COPY model.pth .
COPY spk_xvector.ark .
COPY config.yaml .

COPY requirements.txt .

RUN pip install --no-cache-dir --upgrade -r requirements.txt

RUN rm -rf /root/nltk_data/corpora/cmudict.zip

RUN python -m nltk.downloader cmudict
RUN python -m nltk.downloader averaged_perceptron_tagger

COPY . .

EXPOSE 3100

CMD ["gunicorn", "main:app"]