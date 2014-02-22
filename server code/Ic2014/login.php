<?php
/**
 * Created by IntelliJ IDEA.
 * User: Shantanu
 * Date: 1/2/14
 * Time: 4:32 PM
 */
require_once('connection.php');

$username = mysqli_real_escape_string($connection, $_POST['username']);
$password = mysqli_real_escape_string($connection,$_POST['password']);

if(empty($username)){
    $data=array('error'=>'Username Required');
    echo json_encode($data);
}
else if(empty($password)){
    $data=array('error'=>'Password Required');
    echo json_encode($data);
}
else if(!empty($username)&&(!empty($password))){
    $password=sha1($password);
    $result=mysqli_query($connection,"Select * FROM users where Username = '$username' And Password = '$password' ");
    if(mysqli_num_rows($result) == 1)
    {
        //the log-in is OK so set the username and user ID cookies and redirect to the home page
        $row = mysqli_fetch_array($result);
        if($row['Confirmation']=='true'){
                $data=array('error'=>'Success','UserId'=>$row['Userid'],'Username'=>$row['Username']);
                echo json_encode($data);
        }else{
            $data=array('error'=>'Please verify your email address');
            echo json_encode($data);
        }
    }
    else{
        $data=array('error'=>'Incorrect username or password');
        echo json_encode($data);
    }

}