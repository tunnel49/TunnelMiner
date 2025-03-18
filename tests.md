## Acceptansvilkor

- Skeppet är på en waypoint (!) framför moderskeppet
  (avdockning är klar)
- Det finns en vektor från startpunkten
- Det finns en orientering som skeppet vill ha
- Skeppet kan orientera sig i orienteringsriktningen
- Skeppet kan röra sig frammåt längs vektorn under bibehållen orientering
- Skeppet håller sin position och orientering på vektorn under nominell yttre påverkan
- Skeppet använder borrar för att gräva ut asteroider i sin väg i frammåt riktningen
- Skeppet anspassar hastigheten för att inte kollidera med asteroider.
- Skeppet åker inte allt för långsamt när det inte finns något att kollidera med
- Skeppet kan återvända till waypointen, längs vektorn
- Skeppet undviker kollisioner när det återvänder.

## Implementationsdetaljer

- Vektorn är förlängningen mellan två waypoints i en docknings AI? Anpassat till en konstant 
  längd
- Skeppet har:
    1. sensorer för att detektera asteroider
    2. docknings ai
    3. thrusters i alla kardinalriktningar
    4. gyro
    5. remote control

## Test

1.) Programmet tar en input:
    om inputen är debug skriv ut ett meddelande i programmets output
2.) Programmet har ett remote block, om det inte finns, skriv ut det vid debug
3.) Det finns en sträng med remoteblockets namn, vid programstart läser programmet in objektet med det namnet. Om det finns, skriv ut att det gör det.
4.) Skriv ut remote control blockets namn när det hittats
5.) Skriv ut remote control blockets lokala quarternion(?)
6.) Skriver ut den globala quaternionen för remot control blocket


Skapa periodiska körningar
Skapa en funktion som tar pitch, yaw och roll och sätter override på gyron.

### Stolen code

void ApplyGyroOverride(double pitch_speed, double yaw_speed, double roll_speed, List<IMyGyro> gyro_list, IMyTerminalBlock reference)
{
    var rotationVec = new Vector3D(-pitch_speed, yaw_speed, roll_speed); //because keen does some weird stuff with signs 
    var shipMatrix = reference.WorldMatrix;
    var relativeRotationVec = Vector3D.TransformNormal(rotationVec, shipMatrix);

    foreach (var thisGyro in gyro_list)
    {
        var gyroMatrix = thisGyro.WorldMatrix;
        var transformedRotationVec = Vector3D.TransformNormal(relativeRotationVec, Matrix.Transpose(gyroMatrix));

        thisGyro.Pitch = (float)transformedRotationVec.X;
        thisGyro.Yaw = (float)transformedRotationVec.Y;
        thisGyro.Roll = (float)transformedRotationVec.Z;
        thisGyro.GyroOverride = true;
    }
}

7.) 

8.) Ny funktion test1. som roterar lite åt något håll
