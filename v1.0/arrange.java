//First version of the engine stripped of any plugin system.. to test core functionality

import java.io.*;
import java.util.*;
import java.lang.*;

class iput
{
	int ref,wt,prox;
	public iput(){	wt=0;ref=0;prox=0;}
	
	public iput(int a,int b,int c){	ref = a;wt = b;prox = c;}
	
	public int getref(){return ref;}
	public int getwt() {return wt;}
	public int getprox() {return prox;}
}

class division
{
	int[][] prox;
	
	public division(int n)
	{
		prox = new int[n][2];
		for(int i=0;i<n;i++)
			Arrays.fill(prox[i],0);
	}
	
	public void setprox(iput[] ip)
	{
		for(int i=0;i<ip.length;i++)
		{
			prox[ip[i].getref()][0] = ip[i].getwt();
			prox[ip[i].getref()][1] = ip[i].getprox();
		}
	}
	
	public int getwt(int i){	return prox[i][0];}
	
	public int getprox(int i){	return prox[i][1];}
}

class perms
{
	int siz;
	int[][] mat;
	
	public perms(int n)
	{
		mat = new int[n][2];
		double N = (double)n;
		for(double i=0; i<N;i++)
		{
			mat[(int)i][0] = (int)(N* Math.cos(2*i*Math.PI/N));
			mat[(int)i][1] = (int)(N* Math.sin(2*i*Math.PI/N));
		}
	}
	
	public void permute(division[] d)
	{
		int cobins = 0;		//current object under inspection
		double scores = score(d);
		System.out.println("Init");
		show(scores);
		System.out.println();
		int param = 1;
		boolean cond = true;
		int count = 0;
		while(true)
		{
			cond = true;
			//move obj 1 down .. if check() and improves then print score and continue else move obj back
			mat[cobins][1]+=param;
			if((check(cobins))&&(scores<score(d)))
			{
				scores = score(d);show(scores);
				param = 1;count=0;continue;
			}
			mat[cobins][1]-=param;
			//move obj 1 right .. if check() and improves then print score and continue else move obj back
			mat[cobins][0]+=param;
			if((check(cobins))&&(scores<score(d)))
			{
				scores = score(d);show(scores);
				param = 1;count=0;continue;
			}
			mat[cobins][0]-=param;
			//move obj 1 up .. if check() and improves then print score and continue else move obj back
			mat[cobins][1]-=param;
			if((check(cobins))&&(scores<score(d)))
			{
				scores = score(d);show(scores);
				param = 1;count=0;continue;
			}
			mat[cobins][1]+=param;
			//move obj 1 left .. if check() and improves then print score and continue else move obj back
			mat[cobins][0]-=param;
			if((check(cobins))&&(scores<score(d)))
			{
				scores = score(d);show(scores);
				param = 1;count=0;continue;
			}
			mat[cobins][0]+=param;
			//move obj 1 right 1 down.. if check() and improves then print score and continue else move obj back
			mat[cobins][0]+=param;
			mat[cobins][1]+=param;
			if((check(cobins))&&(scores<score(d)))
			{
				scores = score(d);show(scores);
				param = 1;count=0;continue;
			}
			mat[cobins][0]-=param;
			mat[cobins][1]-=param;
			//move obj 1 right 1 up.. if check() and improves then print score and continue else move obj back
			mat[cobins][0]+=param;
			mat[cobins][1]-=param;
			if((check(cobins))&&(scores<score(d)))
			{
				scores = score(d);show(scores);
				param = 1;count=0;continue;
			}
			mat[cobins][0]-=param;
			mat[cobins][1]+=param;
			//move obj 1 left 1 up.. if check() and improves then print score and continue else move obj back
			mat[cobins][0]-=param;
			mat[cobins][1]-=param;
			if((check(cobins))&&(scores<score(d)))
			{
				scores = score(d);show(scores);
				param = 1;count=0;continue;
			}
			mat[cobins][0]+=param;
			mat[cobins][1]+=param;
			//move obj 1 left 1 down.. if check() and improves then print score and continue else move obj back
			mat[cobins][0]-=param;
			mat[cobins][1]+=param;
			if((check(cobins))&&(scores<score(d)))
			{
				scores = score(d);show(scores);
				param = 1;count=0;continue;
			}
			mat[cobins][0]+=param;
			mat[cobins][1]-=param;
			//iterate to next building
			count++;
			if(count == d.length)
			{
				count = 0;
				param++;
			}
			cobins = (cobins+1)%d.length;
		}
	}
		
	double score(division[] d)
	{
		double score = 0;
		for(int i=0;i<mat.length;i++)
			for(int j=0;j<mat.length;j++)
				score += distance(i,j)*d[i].getprox(j)*d[i].getwt(j)*(-1);
		return score;
	}
	
	boolean check(int x)
	{
		for(int i=0;i<mat.length;i++)
		{
			if((mat[i][0]==mat[x][0])&&(mat[i][1]==mat[x][1])&&(i!=x))
				return false;
		}
		return true;
	}

	double distance(int i,int j)
	{
		double x2 = mat[i][0]-mat[j][0];
		x2*=x2;
		double y2 = mat[i][1]-mat[j][1];
		y2*=y2;
		double sum = x2+y2;
		return Math.sqrt(sum);
	}
	
	public void show(double d)
	{
		System.out.println("Score is : " + d);
		for(int i=0;i<mat.length;i++)
		{
			System.out.println(mat[i][0]+"\t"+mat[i][1]);
		}
	}
}

public class arrange
{
	public static void main(String[] args) throws IOException
	{
		BufferedReader br = new BufferedReader(new InputStreamReader(System.in));
		int nbldg = Integer.parseInt(br.readLine());		//stores number of buildings
		int ref,wt,prox;									//temp variables to store reference, weight and proximity.
		perms permuter = new perms(nbldg);
		division[] bldg = new division[nbldg];				//array for buildings
		for(int i=0;i<nbldg;i++)
		{
			int pbldg = Integer.parseInt(br.readLine());
			iput[] pblip = new iput[pbldg];					//input proximity array for building i
			for(int j=0;j<pbldg;j++)
			{
				String[] s = br.readLine().split(" ");;
				ref = Integer.parseInt(s[0]);
				wt = Integer.parseInt(s[1]);
				prox = Integer.parseInt(s[2]);
				pblip[j] = new iput(ref,wt,prox);
			}
			bldg[i] = new division(nbldg);					//create building i
			bldg[i].setprox(pblip);							//set proximity settings for building i
		}
															//all buildings input and set
		permuter.permute(bldg);
	}
}
