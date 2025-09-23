# Paint 360: XR Redesign of Microsoft Paint

## 1. Microsoft Paint
Paint feels difficult to use on a computer with an overwhelming lack of control, especially when it comes to drawing. Extended reality (XR) environments provide users with more control points and add a level of dynamicness when it comes to creating art.

---

## 2. User Tasks & Goals

### Create art in a 3D space
Express creativity beyond the 2D canvas by working on 3D models through drawing and molding objects. 3D preset shapes can be combined to make complex objects.

### Configuring brushes and tools with gestures
- **Clenching and dragging** → configure brush size and colour  
- **Raycasting** → select tools such as magic selection and colour picker

### Choose custom environments
Users can transform ordinary environments into inspiring backdrops for their creative work. VR allows artworks to be created in custom spaces.

---

## 3. Iterations on Original Idea
1. Creating works in three dimensions rather than always painting on a “floating” 2D canvas.  
2. Creating custom 3D objects by combining preset shapes together using the XR mode.  

**Tutor feedback:**  
A concern was raised about achieving full object molding with Unity. For testing, simpler functionality could be implemented (e.g., squeezing from top and bottom transforms an object into a sphere).

---

## 4. Defining the Concept
*(More sketches and storyboards available on GitHub + initial testing video: [Google Drive link](https://drive.google.com/file/d/1RjzxqyIRTDUasOBzz9LW_-bLxWvnt96C/view?usp=sharing))*  

Our goal is to transform Paint from a flat editor into an immersive 3D art studio where users can paint, draw, and sculpt in an infinite virtual workspace.

- **Launch options:** Choose between creating in 3D or painting on a 2D canvas.  
- **2D Mode:** Uses XR to project the canvas into the real world. Users can paint in their environment or choose preset backgrounds (e.g., natural spaces, architecture, white box).  
- **3D Mode:** Uses VR to allow users to walk around their artwork, paint in mid-air, and mold 3D objects from multiple angles.  

### Interaction Modalities
- **Hand Tracking**  
  - Pinch + swipe → scroll brush size  
  - Pinch + hold → scroll colour picker  
  - Drawing mid-air → 3D brushstrokes  
  - Squeeze → mold 3D objects  

- **Tool Belt**  
  Floating palette with brushes/tools selected through raycasting. Can be attached to the wrist or pinned in 3D space.  

- **Voice Commands**  
  Application commands such as *undo, save, copy & paste*. Reduces reliance on gestures and raycasting.

---

## 5. Testing Plan

### Interactions/Features to Test
- Comfort and ease of **3D painting vs. 2D floating canvas**  
- Gesture-based brush control accuracy  
- Finger tracking accuracy when drawing in 3D  

### Assumptions
- Mid-air painting will feel intuitive  
- Hand tracking gestures will be faster than menus  
- Custom environments will improve creativity  
- 3D object creation is possible but complex molding/detailed construction may be difficult  

### Data to Collect
- Time taken to complete sample artwork  
- Error rate in colour/brush selection  
- User feedback: comfort, ease of use, creative freedom  
- Comparisons with usability of traditional Microsoft Paint (keyboard + mouse)

---
