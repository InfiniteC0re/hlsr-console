const { spawn } = require("child_process");
const fs = require("fs")
const path = require("path");

class HLSRConsole {
	constructor() {	
		this.spawn_paths = [
			path.join(__dirname, "./bin/console.exe"),
			path.join(__dirname, "../../../app.asar.unpacked/node_modules/hlsr-console/bin/console.exe")
		];
	}

	execute(args) {
		if(!args) args = [];
		if(fs.existsSync(this.spawn_paths[1])) {
			this.client = spawn(this.spawn_paths[1], args);	
		}else{
			this.client = spawn(this.spawn_paths[0], args);
		}
	}

	getCustomization() {
		let path1 = path.join(__dirname, "./customization");
		let path2 = path.join(__dirname, "../../../app.asar.unpacked/node_modules/hlsr-console/customization");

		if (fs.existsSync(path1)) {
			return path1;
		} else if(fs.existsSync(path2)) {
			return path2;
		}
	}
}

module.exports = HLSRConsole;