const { spawn } = require("child_process");
const fs = require("fs")
const path = require("path");

class HLSRConsole {
	constructor() {	
		this.spawn_paths = [
			path.join(__dirname, "./bin/HLSRConsole.exe"),
			path.join(__dirname, "../../../app.asar.unpacked/node_modules/hlsr-console/bin/HLSRConsole.exe")
		];
	}

	execute(args, cb) {
		if(!args) args = [];
		let client = null;

		if(fs.existsSync(this.spawn_paths[1])) client = spawn(this.spawn_paths[1], args);	
		else client = spawn(this.spawn_paths[0], args);

		if (cb) client.on("exit", cb);
	}

	getCustomization() {
		let path1 = path.join(__dirname, "./customization");
		let path2 = path.join(__dirname, "../../../app.asar.unpacked/node_modules/hlsr-console/customization");

		if (fs.existsSync(path1)) return path1;
		else if(fs.existsSync(path2)) return path2;
		
		return "";
	}
}

module.exports = HLSRConsole;