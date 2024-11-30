using Orleans;

namespace DistributedDictionary.ActorAbstractions.Terms;

[GenerateSerializer]
public sealed class TermDefinition
{
    [Id(0)]
    public long Id { get; set; }
    [Id(1)]
    public string Simplified { get; set; }
    [Id(2)]
    public string Traditional { get; set; }
    [Id(3)]
    public string Pinyin { get; set; }
    [Id(4)]
    public string Definition { get; set; }
    [Id(5)]
    public string Classifier { get; set; }
    [Id(6)]
    public string Concept { get; set; }
    [Id(7)]
    public int HskLevel { get; set; }
    [Id(8)]
    public string Topic { get; set; }
    [Id(9)]
    public string ParentTopic { get; set; }
    [Id(10)]
    public string Notes { get; set; }
    [Id(11)]
    public double Frequency { get; set; }
    [Id(12)]
    public List<string> PartOfSpeech { get; set; }
}