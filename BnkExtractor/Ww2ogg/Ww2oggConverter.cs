using BnkExtractor.Ww2ogg.Exceptions;
using System;
using System.IO;

namespace BnkExtractor.Ww2ogg;

public static class Ww2oggConverter
{
    public static void PrintUsage()
    {
        Logger.LogVerbose("");
        Logger.LogVerbose("usage: ww2ogg input.wav [-o output.ogg] [--inline-codebooks] [--full-setup]");
        Logger.LogVerbose("                        [--mod-packets | --no-mod-packets]");
        Logger.LogVerbose("                        [--pcb packed_codebooks.bin]");
        Logger.LogVerbose("");
    }

    internal static void Main(int argc, string[] args)
    {
        Logger.LogVerbose($"Audiokinetic Wwise RIFF/RIFX Vorbis to Ogg Vorbis converter");
        Logger.LogVerbose("");

        Ww2oggOptions opt = new Ww2oggOptions();

        try
        {
            opt.ParseArguments(argc, args);
        }
        catch (ArgumentError ae)
        {
            Logger.LogError(ae.ToString());

            PrintUsage();
            return;
        }
        Main(opt);
    }

    internal static void Main(Ww2oggOptions opt)
    {
        try
        {
            Logger.LogVerbose($"Input: {opt.InFilename}");
            using Wwise_RIFF_Vorbis ww = new Wwise_RIFF_Vorbis(opt.InFilename, opt.CodebooksFilename, opt.InlineCodebooks, opt.FullSetup, opt.ForcePacketFormat);

            ww.PrintInfo();
            Logger.LogVerbose($"Output: {opt.OutFilename}");

            try
            {
                using BinaryWriter of = new BinaryWriter(File.Create(opt.OutFilename));
                ww.GenerateOgg(of);
            }
            catch
            {
                // don't leave a half-written .ogg behind for the caller to mistake for a good one
                TryDelete(opt.OutFilename);
                throw;
            }

            Logger.LogVerbose("Done!");
            Logger.LogVerbose("");
        }
        catch (FileOpenException fe)
        {
            Logger.LogError(fe.ToString());
        }
        catch (ParseException pe)
        {
            Logger.LogError(pe.ToString());
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Logger.LogError($"Could not delete partial output {path}: {e.Message}");
        }
    }
}
