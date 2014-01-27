<?php
/**
 * Created by IntelliJ IDEA.
 * User: Shantanu
 * Date: 12/31/13
 * Time: 11:39 PM
 */


function getlasttimestamp($user_id,$authentic_token,$connection){
$last_sync_time='';
$result = mysqli_query($connection, "SELECT * FROM users WHERE userid='$user_id' AND auth_token='$authentic_token'");
if (mysqli_num_rows($result) == 1) {
    $row=mysqli_fetch_array($result);
    if( $row['Confirmation']=='true'){
    $row=mysqli_fetch_array($result);
    $last_sync_time=$row['lastsynctime'];

}
}
return $last_sync_time;
}
function sendtomachine($userid,$timestamp,$connection){
if($timestamp===''){
return "";
    }
$output='';
$tables=array( "Patient", "MutableDetails", "MutableDetailsAllergy",
                "MutableDetailsAddiction", "MutableDetailsOperation", "Address", "AddressZIP", "AddressCity", "AddressState", "MedicalDetails",
                "MedicalDetailsMedicine", "MedicalDetailsVaccine");
$columns=array(1=>"PID,FirstName,LastName,BloodGroup,Sex,Birthday,Image,TimeStamp",
2=>"Married,Occupation,FamilyBackground,Email,Mobile,EmMobile,PID,TimeStamp",
3=>"PID,Allergy,TimeStamp",
4=>"PID,Addiction,TimeStamp",
5=>"PID,Operation,TimeStamp",
6=>"PID,ZIP,Street,TimeStamp",
7=>"ZIP,City,TimeStamp",
8=>"City,State,TimeStamp",
9=>"State,Country,TimeStamp",
10=>"PID,DateVisited,Age,BloodGlucose,SystolicBP,DiastolicBP,DiseaseFound,Height,Weight,Symptoms,BMI,TimeStamp",
11=>"TimeStamp,PID,DateVisited,Medicine",
12=>"TimeStamp,PID,DateVisited,Vaccine"
);
  for($i=0;$i<12;$i++){
  $query="Select ".$columns[$i+1]." FROM " . $tables[$i]." Where ServerTimestamp > '". $timestamp."' And Userid = ".$userid." ;";
  $temp=explode(",",$columns[$i+1]);

  $result=mysqli_query($connection,$query);
    while($row = mysqli_fetch_array($result)){
    $values='';
    for($j=0;$j<count($temp);$j++){
    $values.="'".$row[$temp[$j]]."',";
    }
        $queryformachine="Insert Or Replace into ".$tables[$i]." ( ".$columns[$i+1]." ) values ( ".  substr($values, 0, -1) .");";
        $output.=$queryformachine." ";
}
}
return $output;
}
require_once('connection.php');
$user_id = mysqli_real_escape_string($connection, $_POST['userid']);
$authentic_token = mysqli_real_escape_string($connection, $_POST['auth_token']);
$timestamp=mysqli_real_escape_string($connection, $_POST['timestamp']);
//$user_id=14;
//$authentic_token="28b60a16b55fd531047c";
//$data=array("query"=>sendtomachine($user_id,getlasttimestamp($user_id,$authentic_token,$connection),$connection));
echo  (sendtomachine($user_id,$timestamp,$connection));
?>