<?php
 $data_path = 'alldata39439434.json';
if(isset($_GET['gettoken']))
{
        $state = $_GET['state'];
        $data = [];
        if(file_exists($data_path))
        {
                $data = json_decode( file_get_contents($data_path) , true);
        }

        header("Content-type: application/json");
        if(isset($data[$state]))
        {
                echo json_encode($data[$state]);
                
        }
        else
        {
                echo "{}";
        }
        return;
}
else if(isset($_GET['hastoken']))
{
       
        $data = [];
        if(file_exists($data_path))
        {
                $data = json_decode( file_get_contents($data_path) , true);
        }

        $currentTime = time();
        foreach($data as $key => $value)
        {
                echo $key;
                echo '<br/>';
                if($currentTime - $value['time'] > 100)
                {
                        unset($data[$key]);
                }  
        }

        
        $loginData = $_POST;
        $key = $loginData['state'];
        $data[$key] = $_POST;
        $data[$key]['time'] = time();

        
        file_put_contents($data_path,json_encode($data));
        return;
}


?>
<html>
        <script>
                
                const urlPieces = [location.protocol, '//', location.host, location.pathname];
                let url = urlPieces.join('');
                const xhttp = new XMLHttpRequest();
                xhttp.open("POST", url+"?hastoken");
                xhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
                //alert(window.location.hash);
                
                var data = "";
                if(window.location.hash.length > 20)
                {
                    data = window.location.hash.substr(1);
                }
                else
                {
                    //login failed or cancelled
                    data = window.location.search.substr(1);
                }
                //alert(msg);
                xhttp.send(data); 
                //alert(xhttp.response);
        </script>
        <body><h3>Close this window and go back to the game</h3></body>
</html>
