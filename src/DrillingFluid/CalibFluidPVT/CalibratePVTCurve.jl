

function calibratePVT()
    X =
    [130.6483300589391 1252.780487804878;
    201.37524557956777 995.9512195121952;
    331.041257367387 737.1463414634146;
    475.4420432220039 808.2682926829268;
    600.196463654224 893.219512195122;
    763.2612966601179 482.29268292682923]
    T = [293 323 353 323 293 353] # Temperature in Kelvin
    P = X[:, 1]/100000 # Pressure in Pa
    ρ = X[:, 2]        # Density in kg/m³
    # Populate matrix for interpolation
    M = zeros(6, 6)
    for i in 1:6
        M[i, 1] = 1
        M[i, 2] = T[i]
        M[i, 3] = P[i]
        M[i, 4] = T[i] * P[i]
        M[i, 5] = P[i] * P[i]
        M[i, 6] = T[i] * P[i] * P[i]        
    end
    coeffs = inv(M) *  ρ
    A = coeffs[1]
    B = coeffs[2]
    C = coeffs[3]
    D = coeffs[4]
    E = coeffs[5]
    F = coeffs[6]
    printstyled("================= Interpolation ================\n", color = :green)
    for i in 1:6
        printstyled("coeff $i: ", color = :green)
        printstyled("$(coeffs[i])\n", bold = true, color = :green)
    end
end
function lsqmethed()
    X = [
    75.14734774066798 1298.219512195122;
    187.13163064833006 1207.3414634146343;
    328.5854616895874 1098.6829268292684;
    448.42829076620825 1007.8048780487804;
    578.0943025540275 912.9756097560976;
    708.7426326129666 830;
    125.2455795677799 1049.2926829268292;
    250 962.3658536585366;
    385.5599214145383 869.5121951219512;
    502.4557956777996 790.4878048780488;
    631.139489194499 701.5853658536586;
    780.4518664047151 618.609756097561;
    175.34381139489196 830;
    306.97445972495086 747.0243902439024;
    435.65815324165027 673.9268292682927;
    534.8722986247544 612.6829268292684;
    671.4145383104126 529.7073170731708;
    853.1434184675835 432.90243902439033;
    ]
    T = [293 293 293 293 293 293 323 323 323 323 323 323 353 353 353 353 353 353] # Temperature in Kelvin
    P = X[:, 1]/100000 # Pressure in Pa
    ρ = X[:, 2]        # Density in kg/m³

    M = zeros(18, 6)
    for i in 1:18
        M[i, 1] = 1
        M[i, 2] = T[i]
        M[i, 3] = P[i]
        M[i, 4] = T[i] * P[i]
        M[i, 5] = P[i] * P[i]
        M[i, 6] = T[i] * P[i] * P[i]        
    end
    invMtM = inv(M' * M)
    coeffs =  invMtM * M' *  ρ
    A = coeffs[1]
    B = coeffs[2]
    C = coeffs[3]
    D = coeffs[4]
    E = coeffs[5]
    F = coeffs[6]
    printstyled("================= LSQM ================\n", color = :green)        
    for i in 1:6
        printstyled("coeff $i: ", color = :green)
        printstyled("$(coeffs[i])\n", bold = true, color = :green)
    end 
end
calibratePVT()
lsqmethed()