<?php
/**
 * Created by IntelliJ IDEA.
 * User: Shantanu
 * Date: 1/5/14
 * Time: 9:54 PM
 */

require_once('connection.php');

$username = mysqli_real_escape_string($connection, $_POST['username']);
$password = mysqli_real_escape_string($connection,$_POST['password']);
$organisation= mysqli_real_escape_string($connection,$_POST['organisation']);
$email= mysqli_real_escape_string($connection,$_POST['email']);

if(empty($username)){
    $data=array('error'=>'Username Required');
    echo json_encode($data);
}
else if(empty($password)){
    $data=array('error'=>'Password Required');
    echo json_encode($data);
}
else if(empty($organisation)){
    $data=array('error'=>'Organisation Name Required');
    echo json_encode($data);
}
else if(empty($email)){
    $data=array('error'=>'Email Required');
    echo json_encode($data);
}
else if(!empty($username)&&(!empty($password)&&(!empty($organisation)&&(!empty($email))))){
    $password=sha1($password);
    $auth_token=substr(md5(rand()), 0, 20);
    $query="Insert into users (Username,Password,Email,Organisation,Auth_Token) Values ('$username','$password','$email','$organisation','$auth_token')";
    if(mysqli_query($connection,$query))
    {
        //the log-in is OK so set the username and user ID cookies and redirect to the home page
        $mailoutput=send_email($email,$auth_token,$organisation);
        if($mailoutput){
            $data=array('error'=>'Account Created Successfully Check Your Email For Verification');
            echo json_encode($data);
        }else{
            $data=array('error'=>'Error sending email');
            echo json_encode($data);
        }


    }
    else{
        $data=array('error'=>'Email Address Or Organisation Already Exist Or Username Already taken');
        echo json_encode($data);
    }
}

function send_email($emailaddress,$authtoken,$organisationname){

    require("PHPMailer_5.2.4/class.phpmailer.php");

    $mail = new PHPMailer(true);

    $mail->IsSMTP();  // telling the class to use SMTP
    $mail->SMTPAuth   = true; // SMTP authentication
    $mail->SMTPSecure = "ssl";
    $mail->Host       = "smtp.gmail.com"; // SMTP server
    $mail->Port       = 465; // SMTP Port
    $mail->Username   = "healthorganiser@gmail.com"; // SMTP account username
    $mail->Password   = "imaginecup2014";        // SMTP account password

    $mail->SetFrom('healthorganiser@gmail.com', 'Health Organiser'); // FROM
    $mail->AddReplyTo('healthorganiser@gmail.com', 'Health Organiser'); // Reply TO

    $mail->AddAddress($emailaddress,$organisationname); // recipient email

    $mail->Subject    = "'Health Organiser Verification Email"; // email subject
    $mail->Body       = "Hello ".$organisationname.",
     Please Click on the link for verification.
     http://localhost:63342/Ic2014/signupverification.php?email=".$emailaddress."&auth_token=".$authtoken."
     Regards
     Team Health Organiser";

    if(!$mail->Send()) {
       // echo 'Message was not sent.';
       // echo 'Mailer error: ' . $mail->ErrorInfo;
        return false;
    } else {
        //echo 'Message has been sent.';
        return true;
    }
    return false;
}

?>