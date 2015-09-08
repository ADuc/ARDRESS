#pragma strict
 
 public var rotationSpeed : float = 180.0;
 public var distanceFromCamera : float = 9.7;
 
 var touchedObject : Transform; // private var, left public for testing
 
 
 function Update() 
 {
     // to use mouse inputs in the editor
     // and device touch inputs in build
     #if UNITY_EDITOR
     UsingMouseInputs();
     #else
     UsingTouchInputs();
     #endif
 }
 
 
 // --
 
 
 function UsingTouchInputs() 
 {
     // has the screen got a touch?
     if ( Input.touchCount > 0 )
     {
         // get the touch position on the screen
         var touchPos : Vector3 = new Vector3( Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 0 );
         
         // create a Ray from the touch position
         var ray : Ray = Camera.main.ScreenPointToRay( touchPos );
         
         
         // what is the current state of the touch 0?
         switch( Input.GetTouch(0).phase )
         {
             case TouchPhase.Began :
                 // create a RaycastHit variable to gather Raycast information
                 var hit : RaycastHit;
                 
                 // cast a ray
                 // ideally this would be done using a layermask
                 if ( Physics.Raycast( ray, hit, 1000 ) )
                 {
                     // check the hit object by name
                     // ideally this would be done by checking the tag
                     if ( hit.collider.gameObject.name == "Cube" )
                     {
                         // if the object is a cube
                         // store a reference to the object 
                         touchedObject = hit.transform;
                     }
                 }
             break;
             
             case TouchPhase.Moved :
                 // check if there is a gameObject stored in the variable
                 if ( touchedObject )
                 {
                     // move the object to underneath the touch position
                     touchedObject.position = ray.origin + ( ray.direction * distanceFromCamera );
                     
                     // rotate the object
                     touchedObject.Rotate( 0, 0, rotationSpeed * Time.deltaTime );
                 }
             break;
             
             case TouchPhase.Stationary :
                 // check if there is a gameObject stored in the variable
                 if ( touchedObject )
                 {
                     // rotate the object
                     touchedObject.Rotate( 0, 0, rotationSpeed * Time.deltaTime );
                 }
             break;
             
             case TouchPhase.Ended :
                 // remove the reference to the gameObject
                 touchedObject = null;
             break;
             
             default : // TouchPhase.Cancelled
                 // remove the reference to the gameObject
                 touchedObject = null;
             break;
         }
     }
 }
 
 
 // --
 
 
 // enumerator to emulate touch phases
 enum MousePhase
 {
     Idle,
     Began,
     Stationary,
     Moved,
     Ended,
     Cancelled
 }
 
 var mouseTouchPhase : MousePhase;
 
 
 function UsingMouseInputs() 
 {
     // using mouse as I have no provisioning profile for my touch device
     if ( Input.GetMouseButtonDown(0) )
     {
         mouseTouchPhase = MousePhase.Began;
     }
     else if ( Input.GetMouseButton(0) )
     {
         mouseTouchPhase = MousePhase.Moved;
     }
     else if ( Input.GetMouseButtonUp(0) )
     {
         mouseTouchPhase = MousePhase.Ended;
     }
     else
     {
         mouseTouchPhase = MousePhase.Idle;
         
         return;
     }
     
     
     // get the touch position on the screen
     var touchPos : Vector3 = Input.mousePosition;
     
     // create a Ray from the touch position
     var ray : Ray = Camera.main.ScreenPointToRay( touchPos );
     
     
     // what is the current state of the touch 0?
     switch( mouseTouchPhase )
     {
         case MousePhase.Began :
             // create a RaycastHit variable to gather Raycast information
             var hit : RaycastHit;
             
             // cast a ray
             // ideally this would be done using a layermask
             if ( Physics.Raycast( ray, hit, 1000 ) )
             {
                 // check the hit object by name
                 // ideally this would be done by checking the tag
                 if ( hit.collider.gameObject.name == "Cube" )
                 {
                     // if the object is a cube
                     // store a reference to the object 
                     touchedObject = hit.transform;
                 }
             }
         break;
         
         case MousePhase.Moved :
             // check if there is a gameObject stored in the variable
             if ( touchedObject )
             {
                 // move the object to underneath the touch position
                 touchedObject.position = ray.origin + ( ray.direction * distanceFromCamera );
                 
                 // rotate the object
                 touchedObject.Rotate( 0, 0, rotationSpeed * Time.deltaTime );
             }
         break;
         
         case MousePhase.Ended :
             // remove the reference to the gameObject
             touchedObject = null;
         break;
         
         default :
             
         break;
     }
 }