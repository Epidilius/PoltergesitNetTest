namespace hebertsystems.AVK
{
	// Camera type enumeration for use in potential transitions between cameras.
	// This general camera type isn't used much yet, but may be in the future to simplify and improve transitions, and possibly other things.
	public enum CameraType
	{
		None,					// No type specified
		Orbit,					// All cameras that look at a pivot point near the vehicle and follow the vehicle (orbit, follow, birds eye, etc.) can be considered orbit cameras (for now).
		Cockpit,				// Camera inside the vehicle looking out.
		Stationary				// Stationary camera.
	}
}
