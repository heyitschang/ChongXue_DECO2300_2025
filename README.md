# Paint360: XR Redesign of Microsoft Paint

## 1. Overview
Paint360 is an immersive redesign of Microsoft Paint, built in Unity for the Meta Quest using hand tracking and the Meta Voice SDK.  
It transforms the traditional 2D desktop painting interface into a three-dimensional creative workspace where users can draw, paint, and sculpt in mid-air or on 3D objects.  

This prototype integrates gesture-based controls, voice commands, and an experimental Passthrough Mode that allows users to blend their physical surroundings with the virtual art environment.

---

## 2. User Tasks and Goals

### Create art in a 3D space
- Draw and paint in mid-air directly with hand gestures.  
- Combine preset 3D shapes to form complex objects.  
- Move around the workspace to view and modify artworks from multiple angles.

### Gesture-based brush and tool control
**Right hand**
- Pinch (thumb + index) to draw brush strokes.  
- Open palm to erase existing strokes.  
- Pinch (thumb + middle) to paint 3D objects via raycasting.

**Left hand**
- Combine thumb, index, and middle fingers to mix RGB colours.  
- Colour preview is displayed as a floating sphere near the left hand.

An on-screen HUD briefly displays the active tool and colour after each gesture.

### Voice commands (Meta Voice SDK)
- “New scene” resets the environment.  
- “Mode” toggles between the Virtual Studio and Passthrough Mode.  
Voice commands enable hands-free control and reduce menu interactions.

### Choose creative environments
- **Virtual Mode:** paint within a designed 3D environment.  
- **Passthrough Mode:** use the real-world view as the background for mixed-reality artwork.

---

## 3. Iterations on the Original Idea
Paint360 has developed through several prototype stages, each adding new interaction methods and refining usability.

**Prototype 1:** implemented basic hand-tracked drawing and brush sizing gestures.  
**Prototype 2:** added colour mixing, palm-based erasing, and HUD feedback.  
**Prototype 3:** introduced voice commands, Passthrough Mode, and improved gesture reliability.

**Tutor feedback addressed**
- Simplified 3D shape manipulation to maintain performance.  
- Added clearer feedback for tool changes and improved voice integration.

---

## 4. Defining the Concept

*(More sketches and storyboards available on GitHub + initial testing video: [Google Drive link](https://drive.google.com/file/d/1RjzxqyIRTDUasOBzz9LW_-bLxWvnt96C/view?usp=sharing))*  

**Vision**  
Transform Microsoft Paint into an immersive XR studio that merges gesture, voice, and environmental awareness to support intuitive creative expression.

**Modes**
- **2D (Passthrough):** paint on real-world backdrops using the passthrough camera.  
- **3D (Virtual):** create and explore artwork within a virtual space.

**Interaction Modalities**
- Hand tracking for drawing, colour mixing, erasing, and object painting.  
- Voice input for environment control.  
- Spatial HUD for temporary tool and colour feedback.

---

## 5. Testing Plan

### Features to Evaluate
- Comfort and intuitiveness of mid-air drawing in VR and real-world passthrough.  
- Accuracy and responsiveness of gesture detection.  
- Recognition reliability for voice commands.  
- User perception of creative flow when switching between environments.

### Data Collected
- Time taken to complete drawing tasks.  
- Gesture and voice recognition error rate.  
- Observation of user comfort and movement patterns.  
- Post-test feedback on usability and creative engagement.

---

## 6. Technology Stack
- **Engine:** Unity 2022.3 LTS  
- **SDKs:** Meta XR All-in-One SDK, Meta Voice SDK (Wit.ai)  
- **Platform:** Meta Quest 2 / 3  
- **Input:** Hand tracking and voice  
- **Language:** C#  

---

## 7. Setup and Installation

### Prerequisites
- Unity 2022.3 LTS or newer  
- Meta Quest 2 or Meta Quest 3 headset  
- Android Build Support module installed  
- Meta XR All-in-One SDK imported via Unity Package Manager  
- Meta Voice SDK (Immersive Voice Commands) installed from GitHub or Package Manager  
- Developer Mode enabled on the Quest device  

### Project Setup
1. Clone or download this repository.  
2. Open the project in Unity Hub.  
3. In **Build Settings**, set:
   - Platform: **Android**
   - Target Device: **Meta Quest**
   - Run Device: your connected headset  
4. In **Project Settings → XR Plug-in Management**, enable:
   - **Oculus / Meta XR Plugin** for Android  
5. In **Player Settings**, ensure:
   - Minimum API Level: **Android 10.0 (API Level 29)** or higher  
   - Scripting Backend: **IL2CPP**  
   - Target Architectures: **ARM64**  
6. In the Unity Hierarchy, confirm that:
   - `App Voice Experience` is present and configured.  
   - `VoiceCommandManager` script is attached to a GameObject in the scene.  
   - Environment root or passthrough objects are linked correctly.  

### Building and Running
1. Connect the Quest headset via USB and allow USB debugging.  
2. In Unity, go to **File → Build and Run**.  
3. Wait for deployment; the app will launch automatically in the headset.  

---

## 8. Future Work
- Add additional voice commands such as Undo, Save, and Change Colour.  
- Implement adjustable brush textures and 3D shape scaling.  
- Explore collaborative multi-user painting sessions.

---

## 9. Credits and Acknowledgements
Developed by **Chong Xue**  
Built for the University of Queensland, 2025.  
Includes Meta XR SDKs and Wit.ai Voice SDK for research and prototyping purposes.
