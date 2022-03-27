using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class HybFacebook : MonoBehaviour
{
    public static string accessToken { get; private set; }

    private static string _appId;
    private static string _redirectUri;
    private static string _scope = "email,public_profile";
    private static string _stateParam;

    private static  System.Action<FBLoginResponse> loginCompletedAction;


    public static bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(accessToken);
    }

    public static void Init(string appId,string scope,string redirectUri,System.Action initCompleteCallback)
    {
        _appId = appId;
        _redirectUri = redirectUri;
        _scope = scope;
        if(m_instance == null)
        {
            GameObject o = new GameObject("FB");
            m_instance = o.AddComponent<HybFacebook>();
        }


        accessToken = PlayerPrefs.GetString("at", "");

        if(!string.IsNullOrEmpty(accessToken))
        {
            GraphAPI("/me?fields=id", (response) =>
            {
                if(response.result != UnityWebRequest.Result.Success)
                {
                    accessToken = "";
                }
                else
                {
                    var node = SimpleJSON.JSON.Parse( response.downloadHandler.text);
                    if(node["error"].IsNull)
                    {
                        accessToken = "";
                    }
                }

                initCompleteCallback();
            });
        }
    }


    
    public static void Login(System.Action<FBLoginResponse> loginCompleted)
    {
        accessToken = "";

        loginCompletedAction = loginCompleted;
        _stateParam = SystemInfo.deviceUniqueIdentifier + System.DateTime.UtcNow.ToFileTime();
        string url = $"https://www.facebook.com/v13.0/dialog/oauth?response_type=token&client_id={_appId}&redirect_uri={_redirectUri}&state={_stateParam}&scope={_scope}";
        Application.OpenURL(url);
        m_instance.StartCoroutine(m_instance.CheckForCallback());
        
    }


    public static void GraphAPI(string path,System.Action<UnityWebRequest> callback)
    {
        m_instance.StartCoroutine(m_instance.ProcessGraphAPI(path,callback));
    }

    public static void Logout()
    {
        accessToken = null;
        PlayerPrefs.DeleteKey("at");
        PlayerPrefs.Save();
    }

    private IEnumerator ProcessGraphAPI(string path, System.Action<UnityWebRequest> callback)
    {
        string url = $"https://graph.facebook.com{path}&access_token={accessToken}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        callback(request);
    }

    private IEnumerator CheckForCallback()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(4);
        while(true)
        {
            yield return waitForSeconds;

            WWW www = new WWW($"https://vps.hybriona.com/api/fb/process.php?gettoken&state={_stateParam}");
            yield return www;
            if(string.IsNullOrEmpty(www.error))
            {
                string data = www.text;
                if (data.Length > 5)
                {
                    var responseNode =  SimpleJSON.JSON.Parse(data);
                    if(responseNode["error"] != null)
                    {
                       

                        FBLoginResponse loginResponse = new FBLoginResponse(isError: true, errorDescription: responseNode["error_description"].Value);
                        if (loginCompletedAction != null)
                            loginCompletedAction(loginResponse);
                    }
                    else
                    {
                        accessToken = responseNode["access_token"];
                        PlayerPrefs.SetString("at", accessToken);
                        PlayerPrefs.Save();

                        FBLoginResponse loginResponse = new FBLoginResponse(isError: false, errorDescription:null);
                        if (loginCompletedAction != null)
                            loginCompletedAction(loginResponse);
                    }
                    

                    

                    
                    yield break;

                    //error_reason = user_denied
                    //& error = access_denied
                    //& error_description = Permissions + error.
                }
            }
        }

    }

    private static HybFacebook m_instance;
}
[System.Serializable]
public struct FBLoginResponse
{
    public bool isError { get; private set; }
    public string errorDescription { get; private set; }


    public FBLoginResponse(bool isError, string errorDescription)
    {
        this.isError = isError;
        this.errorDescription = errorDescription;
    }
}