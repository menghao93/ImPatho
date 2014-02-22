<?php
/**
 * Created by IntelliJ IDEA.
 * User: Shantanu
 * Date: 1/6/14
 * Time: 8:01 PM
 */
require_once('connection.php');
$email=mysqli_real_escape_string($connection,$_GET['email']);
$auth_token=mysqli_real_escape_string($connection,$_GET['auth_token']);
$result=mysqli_query($connection,"Select * from users where Email='$email' And Auth_Token='$auth_token'");
if(mysqli_num_rows($result) == 1)
{
    //the log-in is OK so set the username and user ID cookies and redirect to the home page
    $row = mysqli_fetch_array($result);
    $data=array('error'=>'Success','UserId'=>$row['Userid'],'Username'=>$row['Username']);
    mysqli_query($connection,"Update users set confirmation='true'");
    echo json_encode($data);
}
else{
    $data=array('error'=>'Invalid link used');
    echo json_encode($data);
}

?>