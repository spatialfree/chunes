TanjentOGG is a free open source library to decode OGG/Vorbis files
in C#, HaXe and Java

Version History
=======================================================================
2015-05-01  First released version 1.0

Introduction
=======================================================================
The main goal of this library is to make it easy to add software
OGG/Vorbis decoding for applications written in C#, HaXe and Java.

I hope that this library will make the .OGG/Vorbis file format more
accessible as an audio format for game engines, music software and
similar applications.

At its core the library is a port of the floating point libvorbis
reference decoder by the Xiph.org Foundation. 

Example Usage (C#)
=======================================================================
Add the library files located in src\cs\ to your project.

C# Example 1

    byte[] bytes = File.ReadAllBytes("awesome.ogg");
    TanjentOGG.TanjentOGG t = new TanjentOGG.TanjentOGG();
    t.DecodeToFloats(bytes);
    Console.WriteLine(t.Channels + " " + t.SampleRate + " " + t.DecodedFloats.Length);
    
C# Example 2
    
    byte[] bytes = File.ReadAllBytes("awesome.ogg");
    float[] decodedFloats = new float[456];
    TanjentOGG.TanjentOGG t = new TanjentOGG.TanjentOGG();
    while (true)
    {
        int nos = t.DecodeToFloatSamples(bytes, decodedFloats);
        if (nos <= 0) break;
        // ... do something with decodedFloats here!
        // decodedFloats[i] *= 0.5; 
    }
    Console.WriteLine(t.Channels + " " + t.SampleRate + " " + decodedFloats.Length);

A more complete test example for C# is in examples\cs\Program.cs

Example Usage (HaXe)
=======================================================================
Add the library files located in src\haxe\ to your project.

HaXe Example 1 - openfl

    var b:ByteArray = Assets.getBytes("awesome.ogg");
    var fb:Bytes = Bytes.alloc(b.length);           
    for (i in 0 ... b.length) {
        fb.set(i, b.readByte());
    }
    var tanjentOGG:TanjentOGG = new TanjentOGG();
    tanjentOGG.decodeToFloats(fb);
    trace(tanjentOGG.channels + " " + tanjentOGG.sampleRate + " " + tanjentOGG.decodedFloats.length);
    
HaXe Example 2 - openfl

    var b:ByteArray = Assets.getBytes("awesome.ogg");
    var fb:Bytes = Bytes.alloc(b.length);           
    for (i in 0 ... b.length) {
        fb.set(i, b.readByte());
    }
    var tanjentOGG:TanjentOGG = new TanjentOGG();
    var floatSamples:Array<Float> = new Array<Float>();
    for (i in 0 ... 456) floatSamples[i] = 0;
    while (true) {
        var nos:Int = tanjentOGG.decodeToFloatSamples(fb, floatSamples);                
        if (nos <= 0) break;        
        // ... do something with floatSamples here!
        // floatSamples[i] *= 0.5;      
    }
    trace(tanjentOGG.channels + " " + tanjentOGG.sampleRate + " " + floatSamples.length);
    
A more interactive test example is located in the examples\haxe folder.
This is a complete HaXe+OpenFL+Lime project ready to be opened in
FlashDevelop. Before you can compile the project you need to copy
the TanjentOGG source code files from src\haxe into the project folder
(Starfield\src).

The example project has been tested to work on these platforms:

  - Flash - slow decode time (~ 15 s)
  - Nexus 7 (2012) - ok-ish decode time (~ 10 s)
  - Windows - ok decode time (~ 3 s)

The example loads and mixes 4 .OGG files encoded
at 44100, 46 kbit/s. The compressed OGG/Vorbis-files are each
9 seconds long and take about 170 KB on disc. Uncompressed
they are about 4*9*44100*2*2 = 6350400 bytes = 6201 KB of
raw 16-bit PCM-audio data.

To use the example: First click the load button to decode
the four ogg files in "realtime" (to prevent the flash environment
from stalling I have split up the decode in a crude timer callback
function that gets called in intervals of 20 ms). 

When the files have been decoded you can listen/trigger the files
by clicking in one of the four columns in the area below the load button.

Example Usage (Java)
=======================================================================
Add the library files located in src\java\ to your project.

Java Example 1 - libgdx

    // libgdx only used for file reading here
    FileHandle file = Gdx.files.internal("awesome.ogg"); 
    byte[] bytes = file.readBytes();
    TanjentOGG t = new TanjentOGG();
    t.decodeToFloats(bytes);
    String outPut = t.channels + " " + t.sampleRate + " " + t.decodedFloats.length;
    
Java Example 2 - libgdx

    // libgdx only used for file reading here
    FileHandle file = Gdx.files.internal("awesome.ogg");
    byte[] bytes = file.readBytes();
    float[] decodedFloats = new float[456];
    TanjentOGG t = new TanjentOGG();
    while (true)
    {
        int nos = t.decodeToFloatSamples(bytes, decodedFloats);
        if (nos <= 0) break;
        // ... do something with decodedFloats here!
        // decodedFloats[i] *= 0.5;         
    }
    String outPut = t.channels + " " + t.sampleRate + " " + decodedFloats.length;

A more complete test example that uses the libgdx library is in
examples\java\TanjentOGGTest.java. The code in this file can
be copy-and-pasted into a file in the 'core' folder of a typical
libgdx gdx-setup.jar created project.

Design
======================================================================
My ambition with this library has been to make it very easy to decode
OGG Vorbis files!

There are two functions and three variables of interest in the library:

F1: void DecodeToFloats(byte[] fileBytes)
F2: int DecodeToFloatSamples(byte[] fileBytes, float[] floatSamples)

V1: int SampleRate
V2: int Channels
V3: float[] DecodedFloats

The function F1 decodes the entire file in one pass to the array V3
and stores the number of channels in V2 and the sample rate in V1.

The functions F2 decodes the OGG/Vorbis file in several passes by
filling the provided parameter "floatSamples" with
"floatSamples.length" samples. The return value is the actual
number of samples filled. Call F2 in a loop to "stream" the
decoded PCM data - only to stop when the return value is 0 or
negative. During the loop F2 updates V1 continuously
if any changes in sample rate takes place (unlikely).

F1 takes more memory but is easier to use if you only want to
decode a small sample (like a short sound FX) into a single
buffer. When testing it has been found that the memory
requirements for this function might be too much for decoding
an entire 3+ minute song on mobile devices and similar
platforms with limited memory.

For longer files such as wavetable banks or 3+ minute songs it
is usually better to use F2 since this function requires much
less working memory because it decodes the audio in smaller
chunks. It is also possible to call this function in the
background to prevent the app from stalling or becoming
non-responsive for longer decodes.

Development History
=======================================================================
The reason for the existence of this library was my need for a better
sample storage solution in a midi-file playback application.

Building a good sound bank based on uncompressed PCM/WAV samples
made the disk space requirements quite large and a better disk storage
solution was needed. A free compressed format with variable
fidelity settings like OGG/Vorbis was a good solution - but many of
the available software decoders for the .OGG format was unsuitable
for the project (C-code with pointer arithmetic, abandoned
projects, or projects released under questionable/uncompatible
licenses).

Initially a Java port of the Tremor-fixed point library was
developed, but due to the lack if inline functions the performance
of the central MDCT routines was very bad. It was also found that
the reference Tremor C-decoder had some issues decoding some
very low bitrate .OGG files. In contrast, the reference floating
point version provided by libvorbis had much better performance
and no audible problems decoding the tested files.

Once the Java version was written two additional ports to C# 
and HaXe was made.

Typical Decode Times for 9 sample .ogg files
=======================================================================
            Encoding                Length  Compressed  Uncompressed
01.ogg      mono 44.1 239 kbit/s      0:10      145 KB        432 KB
02.ogg      mono 48.0 239 kbit/s      0:18      424 KB        868 KB
03.ogg      stereo 44.1 128 kbit/s    0:06      100 KB        518 KB
04.ogg      stereo 44.1 80 kbit/s     5:07    3 152 KB       25.8 MB
05.ogg      mono 24.0 80 kbit/s       5:07    1 857 KB       7.03 MB
06.ogg      mono 24.0 48 kbit/s       3:45    1 640 KB       5.15 MB
07.ogg      stereo 22.0 171 kbit/s    8:32   10 448 KB       21.5 MB
08.ogg      stereo 44.1 128 kbit/s    0:01       19 KB        116 KB
09.ogg      n/a (random bytes)        n/a       200 KB           n/a

OGG File    C#:Win7/F1       C#:Win7/F2(456)      C#:Win7/F2(8192)
01.ogg           89 ms                 61 ms                 59 ms     
02.ogg          150 ms                145 ms                145 ms   
03.ogg           66 ms                 70 ms                 68 ms
04.ogg         2999 ms               3502 ms               3506 ms  
05.ogg          920 ms                936 ms                934 ms    
06.ogg          699 ms                669 ms                669 ms   
07.ogg         3993 ms               3949 ms               3955 ms  
08.ogg           31 ms                 15 ms                 14 ms   
09.ogg            0 ms                  0 ms                  0 ms

OGG File    HaXe:Flash/F1   HaXe:Flash/F2(456)  HaXe:Flash/F2(8192)
01.ogg            1168 ms              1500 ms              1498 ms
02.ogg            2765 ms              3092 ms              3098 ms
03.ogg            1447 ms              2025 ms              1902 ms
04.ogg           timeout!             timeout!             timeout!
05.ogg           timeout!             timeout!             timeout!
06.ogg           timeout!             timeout!             timeout!
07.ogg           timeout!             timeout!             timeout!
08.ogg             285 ms               422 ms               428 ms
09.ogg               1 ms                 0 ms                 0 ms

OGG File    HaXe:Win7/F1     HaXe:Win7/F2(456)    HaXe:Win7/F2(8192)
01.ogg            140 ms                124 ms                140 ms
02.ogg            312 ms                218 ms                234 ms
03.ogg            156 ms                187 ms                156 ms
04.ogg          35693 ms               9952 ms               9687 ms
05.ogg           1497 ms               2496 ms               2340 ms
06.ogg           2371 ms                858 ms                904 ms
07.ogg          10389 ms               6302 ms               6146 ms
08.ogg             78 ms                 31 ms                 31 ms
09.ogg              0 ms                  0 ms                  0 ms

OGG File    Java:Win7/F1     Java:Win7/F2(456)    Java:Win7/F2(8192)
01.ogg             74 ms                154 ms                115 ms
02.ogg             57 ms                 65 ms                 62 ms
03.ogg             60 ms                 32 ms                 60 ms
04.ogg            974 ms               1271 ms               1183 ms
05.ogg            298 ms                380 ms                304 ms
06.ogg            210 ms                256 ms                206 ms
07.ogg           1372 ms               1514 ms               1433 ms
08.ogg              6 ms                  6 ms                  5 ms
09.ogg              1 ms                  0 ms                  0 ms

OGG File    Java:Nexus7/F1  Java:Nexus7/F2(456) Java:Nexus7/F2(8192)
01.ogg              759 ms              1032 ms               980 ms
02.ogg             1558 ms              2050 ms              2099 ms
03.ogg              964 ms              1147 ms              1148 ms
04.ogg      out of memory!             47472 ms             47777 ms
05.ogg      out of memory!             12679 ms             12663 ms     
06.ogg      out of memory!              7378 ms              7400 ms
07.ogg      out of memory!             46788 ms             46864 ms
08.ogg              238 ms               204 ms               204 ms
09.ogg                0 ms                 0 ms                 0 ms

Downloads
=======================================================================
TanjentOGG is written and maintained by Jonas Murman at Tanjent.

The initial development started in the spring of 2015 and the first
version was released in May 2015.

The latest version of this library will be available at
http://www.tanjent.se/labs/tanjentogg.html

Reporting Issues
=======================================================================
If you find a OGG/Vorbis file that does not decode correctly, please
visit the homepage and let me know!

If you manage to find a problem and correct it I would be very happy to
incorporate your changes to the library, provided you are ready to
license them under the BSD-license.

License
=======================================================================
TanjentOGG is released under the 3-clause BSD license. This means that
you can use it free of charge in commercial and non-commercial projects.
Please read license.txt for the full license.

Since the library in its essence is a port of the OGG/Vorbis decoding
parts of the two C-code libraries libogg and libvorbis the original
BSD license by the Xiph.org Foundation as specified in the file COPYING
must also be met.

Please note that the Xiph.org Foundation has NOT endorsed this port.