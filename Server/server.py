from flask import Flask, request, jsonify
import time
import os
import glob
import json
from datetime import datetime 
from routes import process_request_from_flask, summarize_whole_conversation
from pydub import AudioSegment

app = Flask(__name__)

UPLOAD_FOLDER = 'uploads'
os.makedirs(UPLOAD_FOLDER, exist_ok=True)

# 接收 Unity 的.wav
@app.route('/upload_wav', methods=['POST'])
def upload_wav():
    data = request.data
    is_last = request.args.get('is_last', 'false').lower() == 'true' 

    timestamp = int(time.time())
    wav_filename = f"uploaded_{timestamp}.wav"
    wav_path = os.path.join(UPLOAD_FOLDER, wav_filename)

    with open(wav_path, "wb") as f:
        f.write(data)

    # 轉 mp3
    mp3_filename = f"uploaded_{timestamp}.mp3"
    mp3_path = os.path.join(UPLOAD_FOLDER, mp3_filename)

    audio_segment = AudioSegment.from_wav(wav_path)
    audio_segment.export(mp3_path, format="mp3")

    # 呼叫 Gemini 分析
    result = process_request_from_flask(mp3_path)

    if is_last:
        summarize_whole_conversation()
        print("收到 is_last=true，開始整理總摘要！")

    os.remove(mp3_path)
    return jsonify(result)

# 傳送 JSON 給 Unity
@app.route('/get_data', methods=['GET'])
def get_data():
    folder = 'C:/OpenHCI/conversations_history'
    list_of_files = glob.glob(os.path.join(folder, 'uploaded_*.json'))
    if not list_of_files:
        return jsonify({"error": "No JSON files found."})

    latest_file = max(list_of_files, key=os.path.getctime) 
    with open(latest_file, encoding='utf-8') as f:
        data = json.load(f)

    return jsonify(data)


if __name__ == '__main__':
    print("🚀 Flask server running at http://127.0.0.1:5000")
    app.run(host='127.0.0.1', port=5000)
