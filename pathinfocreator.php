<?php 
	echo 'Generating';
	flush();
	
    function index($dir2) {
        $open = opendir($dir2);
        $out = array();

        while($dir = readdir($open)) {
            if($dir == '.' || $dir == '..') continue;
            if(is_dir($dir2 . '/' . $dir)) {
                $out = array_merge($out, index($dir2 . '/' . $dir));
            } else { 
                $out[] = $dir2 . '/' . $dir;
            }
        }
        return $out;
    }
    
    $o = index('metin');
	
	$out = '<pathinfo>';
	for($i=0;$i<count($o);$i++) {
		echo '.';
		flush();
	    $out .= '<data name="' . str_replace('/', '\\', substr($o[$i], 6)) . '" hash="' . strtoupper(md5_file($o[$i])) . '" />' . "\n";
	}
	$out .= '</pathinfo>';
	
	$f = fopen('pathinfo.xml', 'w');
	fwrite($f, $out);
	fclose($f);
