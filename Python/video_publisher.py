import cv2

cap = cv2.VideoCapture("Stereo Video.mp4")

if not cap.isOpened():
    print("Failed to open video")
    exit()

# Don't let OpenCV resize the window
cv2.namedWindow("Stereo Video (Raw)", cv2.WINDOW_NORMAL)

while True:
    ret, frame = cap.read()
    if not ret:
        print("End of video or failed to read.")
        break

    # Force the window size to match the video
    cv2.imshow("Stereo Video (Raw)", frame)

    # Exit on Esc key
    if cv2.waitKey(1) & 0xFF == 27:
        break

cap.release()
cv2.destroyAllWindows()
