package com.HeavyInsertionExt.sfs2x;

import com.smartfoxserver.v2.core.SFSEventType;
import com.smartfoxserver.v2.extensions.SFSExtension;

public class HeavyInsertionExt extends SFSExtension {

	@Override
	public void init() {
		//this.addRequestHandler("PhysicsList", PhysicsDeterminer.class);
		this.addEventHandler(SFSEventType.USER_JOIN_ROOM, PhysicsDeterminer.class);
		this.addEventHandler(SFSEventType.USER_LEAVE_ROOM, PhysicsDeterminer.class);
		this.addEventHandler(SFSEventType.USER_DISCONNECT, PhysicsDeterminer.class);
	}
}
