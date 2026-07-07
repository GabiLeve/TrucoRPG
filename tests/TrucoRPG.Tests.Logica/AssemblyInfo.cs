using Xunit;

// Los tests usan estado estático compartido (PartidaMemoriaServicio,
// seams RandomNext de MaquinaServicio2v2/3v3 y DecisionMaquinaServicio).
// Correr clases de test en paralelo produce resultados intermitentes,
// así que desactivamos la paralelización de xUnit para este assembly.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
