<?php
/**
 * Created by IntelliJ IDEA.
 * User: Shantanu
 * Date: 1/2/14
 * Time: 4:29 PM
 */
require("constants.php");


//connecting the database
$connection = mysqli_connect($host, $user, $pass);

if (mysqli_connect_errno()) {
    printf("Connect failed: %s\n", mysqli_connect_error());
    exit();
}

//selecting a database to use
$db_select = mysqli_select_db($connection, $db);
?>