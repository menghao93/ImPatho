<?php
/**
 * Created by IntelliJ IDEA.
 * User: Shantanu
 * Date: 1/9/14
 * Time: 1:06 PM
 */
require_once('connection.php');
$sql_statement=$_POST['updatestatements'];
$userid=$_POST['userid'];
$auth_token=$_POST['auth_token'];
//echo $auth_token.$userid."<br>".$sql_statement;
$query="SELECT * FROM users where Userid='$userid' And Auth_Token='$auth_token'";
$result=mysqli_query($connection,$query);
if(mysqli_num_rows($result) == 1)
    {
        //the log-in is OK so set the username and user ID cookies and redirect to the home page
        $row = mysqli_fetch_array($result);
        if($row['Confirmation']=='true'){
               //update_table($sql_statement);
               $result=mysqli_multi_query($connection,$sql_statement);
               if(!$result){
                $data=array('error'=>'Error while syncing');
                echo json_encode($data);
               }else{
                $data=array('error'=>'Sync successfull');
                echo json_encode($data);
               }
        }else{
            $data=array('error'=>'Please verify your email address');
            echo json_encode($data);
        }
    }else{
     $data=array('error'=>'Invalid UserID');
            echo json_encode($data);
    }

function sendtomachine(){
$columns=array( "Patient", "MutableDetails", "MutableDetailsAllergy",
                "MutableDetailsAddiction", "MutableDetailsOperation", "Address", "AddressZIP", "AddressCity", "AddressState", "MedicalDetails",
                "MedicalDetailsMedicine", "MedicalDetailsVaccine");
}
?>