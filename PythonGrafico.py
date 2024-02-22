import matplotlib.pyplot as plt
import numpy as np
import pandas as pd

# Leggi i dati dal file CSV
file_path = "C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\RegressionResults\\Results.csv"
#file_path = "C:\\Users\\aless\\OneDrive\\Desktop\\UNI\\TIROCINIO\\LavoroFinale\\ClusteringResults\\Results.csv"
df = pd.read_csv(file_path)
print(df.info())

# Estrai i dati necessari dalle colonne del dataframe
num_campioni = df['NumSamples']
accuratezza = df['Accuracy']
sensibilita = df['Sensitivity']
specificita = df['Specificity']
g_mean = df['G-Mean']

# Posizione degli indici sull'asse x
indici = np.arange(len(num_campioni))

# GRAFICO ANDAMENTO
plt.figure(figsize=(10, 6))  # Imposta le dimensioni della figura
plt.plot(num_campioni, accuratezza, label='Accuratezza', marker='o', color='blue', linewidth=2)
plt.plot(num_campioni, sensibilita, label='Sensibilit\u00E0', marker='s', color='orange', linewidth=2)
plt.plot(num_campioni, specificita, label='Specificit\u00E0', marker='^', color='green', linewidth=2)
plt.plot(num_campioni, g_mean, label='G-Mean', marker='D', color='red', linewidth=2)

# Etichette degli assi e titolo
plt.xlabel('Numero di campioni sintetici', fontsize=14)
plt.ylabel('Valore', fontsize=14)
plt.title('Andamento delle metriche in relazione al numero di campioni sintetici', fontsize=16)

# Aggiungi legenda
plt.legend()

# Visualizza il grafico
plt.grid(True, linestyle='--', alpha=0.7)  # Aggiungi una griglia più sottile e trasparente
plt.tight_layout()  # Evita che le etichette si sovrappongano
plt.show()

#GRAFICO A BARRE

# Larghezza delle barre
larghezza_barre = 0.18

plt.bar(indici, accuratezza, larghezza_barre, label='Accuratezza', color='blue', edgecolor='black')
plt.bar(indici - larghezza_barre, sensibilita, larghezza_barre, label='Sensibilit\u00E0', color='orange', edgecolor='black')
plt.bar(indici + larghezza_barre, specificita, larghezza_barre, label='Specificit\u00E0', color='green', edgecolor='black')
plt.bar(indici + 2 * larghezza_barre, g_mean, larghezza_barre, label='G-Mean', color='red', edgecolor='black')

# Etichette degli assi e titolo
plt.xlabel('Numero di campioni sintetici')
plt.ylabel('Valore')
plt.title('Andamento delle metriche in relazione al numero di campioni sintetici')

# Etichette sull'asse x
plt.xticks(indici + (larghezza_barre/2), num_campioni)


# Aggiungi legenda
plt.legend()

# Visualizza il grafico
plt.grid(False)  # Aggiungi una griglia più sottile e trasparente
plt.tight_layout()  # Evita che le etichette si sovrappongano
plt.show()