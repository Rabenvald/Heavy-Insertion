package com.HeavyInsertionExt.sfs2x;

import java.util.List;

import com.smartfoxserver.v2.core.ISFSEvent;
import com.smartfoxserver.v2.core.SFSEventParam;
import com.smartfoxserver.v2.entities.Room;
import com.smartfoxserver.v2.entities.User;
import com.smartfoxserver.v2.entities.data.SFSArray;
import com.smartfoxserver.v2.entities.data.SFSObject;
import com.smartfoxserver.v2.exceptions.SFSException;
import com.smartfoxserver.v2.extensions.BaseServerEventHandler;

public class PhysicsDeterminer extends BaseServerEventHandler {
	User[] users;
	SFSArray userIds; //temporary since I can't seem to convert easily from an array to an SFSArray, used to store userIds and send that back.
	
	public void handleServerEvent(ISFSEvent event) throws SFSException {
		Room theRoom = (Room) event.getParameter(SFSEventParam.ROOM);
		List<User> temp = theRoom.getUserList();
		users = (User[]) temp.toArray(); //setting the user list to an array
		
		//sort the array by pings
		users = mergeSort(users);
		
		//convert User[] to an SFSArray 
		//******* at the moment just copies only ids from the sorted users array to SFSArray of userIds *****//
		for(int i=0; i<users.length; i++){
			userIds.addInt(users[i].getId());
		}
		
		//create and send an object with the array to everyone in the room
		SFSObject obj = SFSObject.newInstance();
		obj.putSFSArray("hierarchy", userIds);
		send("data",obj, temp);
	}
	
	//recursive sort to sort the users by ping
	private User[] mergeSort(User[] u){
		int i,
			iL = 0,
			iR = 0,
			ping,
			midIndex = u.length/2,
			middle = findMidValue(u);
		User[] left = new User[midIndex], right = new User[midIndex];
		User aU;
		
		//go through each user and organize the list of those who have the lowest average ping
		if(u.length > 1){
			//divide up the lists between left and right based on ping
			for(i=0; i<u.length;i++){
				aU = u[i];
				ping = aU.getVariable("ping").getIntValue();
				if(ping < middle){
					//add item to left
					left[iL] = u[i];
					iL++;
				}
				else{
					//add item to right
					right[iR] = u[i];
					iR++;
				}
			}
			
			//sort down on the list
			left = mergeSort(left);
			right = mergeSort(right);
			
			//start merging the lists
			return merge(left,right);
		}
		else{
			return u;
		}
	}
	
	//merges two lists together and returns the merged list
	private User[] merge(User[] l, User[] r){
		User[] result = new User[10];
		while(l.length > 0 || r.length > 0){
			//if both left and right exist
			if(l.length > 0 && r.length > 0){
				//if the first ping value of left is less than right
				if(l[0].getVariable("ping").getIntValue() <= r[0].getVariable("ping").getIntValue()){
					//append left to result
					System.arraycopy(l, 0, result, result.length-1, l.length);
				}
				else{
					//append right to results
					System.arraycopy(r, 0, result, result.length-1, r.length);
				}
			}
			//if only left exists
			else if(l.length > 0){
				//append left to result
				System.arraycopy(l, 0, result, result.length-1, l.length);
			}
			//if only right exists
			else if(r.length > 0){
				//append right to result
				System.arraycopy(r, 0, result, result.length-1, r.length);
			}
		}
		return result;
	}
	
	//finding the median ping value of the users to be used to test against for the sort
	private int findMidValue(User[] u){
		int mid = 0;
		for(int i=0;i<u.length;i++){
			mid += u[i].getVariable("ping").getIntValue();
		}
		mid = mid/u.length;
		return mid;
	}
}