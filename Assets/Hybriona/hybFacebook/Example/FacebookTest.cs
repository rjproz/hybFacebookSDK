using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
public class FacebookTest : MonoBehaviour
{
    // Start is called before the first frame update
    public RawImage picture;
    public Text nameTarget;
    public Text logText;
    void Start()
    {
        string redirectUrl = "https://vps.hybriona.com/api/fb/process.php";
   
        HybFacebook.Init(appId: "1428281700717324", redirectUri: redirectUrl, scope:"email,public_profile",initCompleteCallback: ()=>
        {
            if (HybFacebook.IsLoggedIn())
            {
                LoadUserData();
            }
        });

       
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            HybFacebook.Login(LoginComplete);
        }

        if (HybFacebook.IsLoggedIn())
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                HybFacebook.OpenShareDialog("https://www.hybriona.com");
            }
        }
    }


    void LoginComplete(FBLoginResponse loginReponse)
    {
        if (loginReponse.isError)
        {
            Log("Login failed with " + loginReponse.errorDescription, true);
            Debug.LogError("Login failed with "+loginReponse.errorDescription);
        }
        else
        {
            Log("Login successful");
            Debug.Log("Login successful");
            LoadUserData();
        }
    }

    private void LoadUserData()
    {
        HybFacebook.GraphAPI("/me/picture?type=large", (request) =>{

            Texture2D img = new Texture2D(1, 1, TextureFormat.RGB24, false);
            img.LoadImage(request.downloadHandler.data);
            img.Apply();
            picture.texture = img;
        });

        HybFacebook.GraphAPI("/me?fields=id,name,email", (request) => {

            var node = JSON.Parse(request.downloadHandler.text);
            nameTarget.text = node["name"].Value;
        });
    }


    private void Log(string msg, bool error = false)
    {
        if (!error)
        {
            logText.text = msg + "\n" + logText.text;
        }
        else
        {
            logText.text = "<color=#FF3B3B>" + msg + "</color>\n" + logText.text;
        }
    }
}
