package md5f50c71c2d06e585480b3d01185f08138;


public class BeaconScan
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.bluetooth.BluetoothAdapter.LeScanCallback
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onLeScan:(Landroid/bluetooth/BluetoothDevice;I[B)V:GetOnLeScan_Landroid_bluetooth_BluetoothDevice_IarrayBHandler:Android.Bluetooth.BluetoothAdapter/ILeScanCallbackInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("IndoorNavigation.Droid.BeaconScan, IndoorNavigation.Android", BeaconScan.class, __md_methods);
	}


	public BeaconScan ()
	{
		super ();
		if (getClass () == BeaconScan.class)
			mono.android.TypeManager.Activate ("IndoorNavigation.Droid.BeaconScan, IndoorNavigation.Android", "", this, new java.lang.Object[] {  });
	}


	public void onLeScan (android.bluetooth.BluetoothDevice p0, int p1, byte[] p2)
	{
		n_onLeScan (p0, p1, p2);
	}

	private native void n_onLeScan (android.bluetooth.BluetoothDevice p0, int p1, byte[] p2);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
