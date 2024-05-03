## Name
maps-upm

## Description
The Maps UPM is the Unity package needed in order to use Maps features from Lightship ARDK in your Unity application. This file can be be brought into your project by using the Unity Package Manager. To use Lightship Maps for Unity, you must download the Lightship Maps SDK Unity package and save it to your computer. The Unity package comes as a compressed tarball that you can add to a Unity project directly via the Package Manager. Detailed steps to install the Maps UPM can be found in our [developer documentation for Installing the Lightship Maps SDK](https://lightship.dev/docs/maps/install/). These steps are also noted below:

### Installing the Lightship Maps SDK with a URL
1. In your Unity project open the **Package Manager** by selecting **Window > Package Manager**. 
	- From the plus menu on the Package Manager tab, select **Add package from git URL...**
	- Enter `https://github.com/niantic-lightship/maps-upm.git`. 
	- Click **Yes** to activate the new Input System Package for AR Foundation 5.0 (if prompted)

### Installing the Lightship Maps SDK from Tarball
1. Download the plugin packages (`.tgz`) from the latest release
	- [maps-upm](https://github.com/niantic-lightship/maps-upm/releases/latest)
2. In your Unity project open the **Package Manager** by selecting **Window > Package Manager**. 
	- From the plus menu on the Package Manager tab, select **Add package from tarball...**
	- Navigate to where you downloaded the Maps UPM, select the `.tgz` file you downloaded, and press **Open**. This will install the package in your project's **Packages** folder as the **Niantic Lightship Maps SDK** folder. 
	- Click **Yes** to activate the new Input System Package for AR Foundation 5.0 (if prompted). 

## More Information on the Lightship Maps SDK
- [Lightship Maps Developer Documentation](https://lightship.dev/docs/maps/)
- [Getting Started with Lightship Maps](https://lightship.dev/docs/maps/getting_started/)
- [Lightship Maps Sample Projects](https://lightship.dev/docs/maps/sample_projects/)
- [Guide of Lightship Maps Features](https://lightship.dev/docs/maps/unity/)

## Support
For any other issues, [contact us](https://lightship.dev/docs/ardk/contact_us/) on Discord or the Lightship forums! Before reaching out, open the Console Log by holding three touches on your device's screen for three seconds, then take a screenshot and post it along with a description of your issue.
