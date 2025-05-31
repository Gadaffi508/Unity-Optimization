import cv2
import mediapipe as mp
import socket
import json

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
unity_landmark_port = ('127.0.0.1', 5053)
unity_video_port = ('127.0.0.1', 5054)

mp_hands = mp.solutions.hands
mp_drawing = mp.solutions.drawing_utils
hands = mp_hands.Hands(static_image_mode=False, max_num_hands=2,
                       min_detection_confidence=0.7, min_tracking_confidence=0.7)

cap = cv2.VideoCapture(0)

while True:
    success, frame = cap.read()
    if not success:
        continue

    frame = cv2.flip(frame, 1)
    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = hands.process(rgb)

    hand_data = {"hands": []}

    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            landmarks = []
            for lm in hand_landmarks.landmark:
                landmarks.append({"x": lm.x, "y": lm.y, "z": lm.z})
            hand_data["hands"].append({"landmarks": landmarks})
            mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

    # LANDMARK VERİSİ GÖNDER
    message = json.dumps(hand_data).encode()
    sock.sendto(message, unity_landmark_port)

    # KAMERA GÖRÜNTÜSÜ GÖNDER
    resized = cv2.resize(frame, (320, 240))
    _, buffer = cv2.imencode('.jpg', resized)
    sock.sendto(buffer.tobytes(), unity_video_port)

    cv2.imshow("Multi-Hand Tracking", frame)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()