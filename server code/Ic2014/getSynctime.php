<?php
/**
 * Created by IntelliJ IDEA.
 * User: Shantanu
 * Date: 1/8/14
 * Time: 9:54 PM
 */
require_once('connection.php');
$user_id = mysqli_real_escape_string($connection, $_POST['userid']);
$authentic_token = mysqli_real_escape_string($connection, $_POST['auth_token']);
$result = mysqli_query($connection, "SELECT * FROM users WHERE userid='$user_id' AND auth_token='$authentic_token'");
if (mysqli_num_rows($result) == 1) {
    if($row['Confirmation']=='true'){
    $row=mysqli_fetch_array($result);
    $last_sync_time=$row['lastsynctime'];
        $data=array('error'=>'Success','sync_time'=>$last_sync_time);
        echo json_encode($data);
    }else{
        $data = array('error' => 'Please Validate email id');
        echo json_encode($data);
    }
} else {
    $data = array('error' => 'Incorrect authentication token');
    echo json_encode($data);
}
?>