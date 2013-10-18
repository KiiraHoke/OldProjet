

function Update () {if (Input.GetKey("a") ) {rigidbody.AddForce (Vector3.right * -10); }if (Input.GetKey("d") ) {rigidbody.AddForce (Vector3.right * 10); }}